using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MassTransit;
using Microsoft.Extensions.Caching.Distributed;
using AccountService.Domain.Entities;
using AccountService.Domain.Repositories;
using AccountService.Application.DTOs;
using SharedEvents;
using SharedKernel.Domain;

namespace AccountService.Application.Commands;

public record CreateAccountCommand(
    Guid CustomerId,
    string AccountType,
    decimal InitialBalance) : IRequest<AccountDto>;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IDistributedCache _cache;

    public CreateAccountCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        IPublishEndpoint publishEndpoint,
        IDistributedCache cache)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _cache = cache;
    }

    public async Task<AccountDto> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        if (request.InitialBalance < 0)
        {
            throw new DomainException("Initial balance cannot be negative.", "INVALID_INITIAL_BALANCE");
        }

        // Generate unique account number
        string accountNumber = string.Empty;
        bool isUnique = false;
        var random = new Random();
        
        while (!isUnique)
        {
            var prefix = request.AccountType.Equals("Savings", StringComparison.OrdinalIgnoreCase) ? "10" : "20";
            var remainingDigits = random.Next(10000000, 99999999).ToString();
            accountNumber = prefix + remainingDigits;

            var existing = await _accountRepository.GetByAccountNumberAsync(accountNumber);
            if (existing == null)
            {
                isUnique = true;
            }
        }

        var account = new Account(request.CustomerId, accountNumber, request.AccountType, request.InitialBalance);

        await _accountRepository.AddAsync(account);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache balance in Redis (24 hour TTL)
        var balanceKey = $"account_balance_{account.AccountNumber}";
        await _cache.SetStringAsync(balanceKey, account.Balance.ToString(), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        }, cancellationToken);

        // Publish event to RabbitMQ
        await _publishEndpoint.Publish(new AccountCreatedEvent(
            account.Id,
            account.CustomerId,
            account.AccountNumber,
            account.AccountType,
            account.Balance,
            account.CreatedAt
        ), cancellationToken);

        return new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            Status = account.Status
        };
    }
}
