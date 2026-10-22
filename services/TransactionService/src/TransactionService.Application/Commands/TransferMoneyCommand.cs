using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Repositories;
using SharedEvents;
using SharedKernel.Domain;

namespace TransactionService.Application.Commands;

public record TransferMoneyCommand(
    string SourceAccountNumber,
    string TargetAccountNumber,
    decimal Amount,
    string Description) : IRequest<string>;

public class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, string>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public TransferMoneyCommandHandler(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IDistributedCache cache)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<string> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        var referenceNumber = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var source = await _accountRepository.GetByAccountNumberAsync(request.SourceAccountNumber);
            if (source == null) 
                throw new DomainException("Source account not found.", "SOURCE_ACCOUNT_NOT_FOUND");

            var target = await _accountRepository.GetByAccountNumberAsync(request.TargetAccountNumber);
            if (target == null) 
                throw new DomainException("Target account not found.", "TARGET_ACCOUNT_NOT_FOUND");

            if (source.AccountNumber == target.AccountNumber)
            {
                throw new DomainException("Cannot transfer to the same account.", "SAME_ACCOUNT_TRANSFER");
            }

            source.Debit(request.Amount);
            target.Credit(request.Amount);

            var txn = new Transaction(
                request.SourceAccountNumber,
                request.TargetAccountNumber,
                request.Amount,
                "Transfer",
                referenceNumber,
                request.Description,
                "Success"
            );

            _accountRepository.Update(source);
            _accountRepository.Update(target);
            await _transactionRepository.AddAsync(txn);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Invalidate Redis cache (Fail-safe)
            try
            {
                await _cache.RemoveAsync($"account_balance_{source.AccountNumber}", cancellationToken);
                await _cache.RemoveAsync($"account_balance_{target.AccountNumber}", cancellationToken);
            }
            catch
            {
                // Silently ignore cache error if Redis server is offline
            }

            // Publish success event (Fail-safe for offline RabbitMQ)
            try
            {
                await _publishEndpoint.Publish(new MoneyTransferredEvent(
                    txn.Id,
                    source.AccountNumber,
                    target.AccountNumber,
                    txn.Amount,
                    txn.Description,
                    txn.ReferenceNumber,
                    txn.CreatedAt
                ), cancellationToken);
            }
            catch
            {
                // Ignore queue timeout if RabbitMQ broker is offline locally
            }

            return referenceNumber;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            // Publish failure event
            await _publishEndpoint.Publish(new MoneyTransferFailedEvent(
                Guid.NewGuid(),
                request.SourceAccountNumber,
                request.TargetAccountNumber,
                request.Amount,
                ex.Message,
                DateTime.UtcNow
            ), cancellationToken);

            throw;
        }
    }
}
