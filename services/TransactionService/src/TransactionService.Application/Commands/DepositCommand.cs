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

public record DepositCommand(
    string AccountNumber,
    decimal Amount,
    string Description) : IRequest<string>;

public class DepositCommandHandler : IRequestHandler<DepositCommand, string>
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public DepositCommandHandler(
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

    public async Task<string> Handle(DepositCommand request, CancellationToken cancellationToken)
    {
        var referenceNumber = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var account = await _accountRepository.GetByAccountNumberAsync(request.AccountNumber);
            if (account == null) 
                throw new DomainException("Account not found.", "ACCOUNT_NOT_FOUND");

            account.Credit(request.Amount);

            var txn = new Transaction(
                string.Empty,
                request.AccountNumber,
                request.Amount,
                "Deposit",
                referenceNumber,
                request.Description,
                "Success"
            );

            _accountRepository.Update(account);
            await _transactionRepository.AddAsync(txn);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            // Invalidate Redis cache (Fail-safe)
            try
            {
                await _cache.RemoveAsync($"account_balance_{account.AccountNumber}", cancellationToken);
            }
            catch
            {
                // Silently ignore cache error if Redis server is offline
            }

            // Publish event (Fail-safe for offline RabbitMQ)
            try
            {
                await _publishEndpoint.Publish(new DepositCompletedEvent(
                    txn.Id,
                    account.AccountNumber,
                    txn.Amount,
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
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
