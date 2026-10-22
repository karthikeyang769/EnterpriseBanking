using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using AccountService.Domain.Repositories;
using AccountService.Application.DTOs;
using SharedKernel.Domain;

namespace AccountService.Application.Queries;

public record GetAccountByIdQuery(Guid AccountId) : IRequest<AccountDto>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IDistributedCache _cache;

    public GetAccountByIdQueryHandler(IAccountRepository accountRepository, IDistributedCache cache)
    {
        _accountRepository = accountRepository;
        _cache = cache;
    }

    public async Task<AccountDto> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"account_profile_{request.AccountId}";

        // Check Redis cache first
        var cachedData = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedData))
        {
            var cachedDto = JsonSerializer.Deserialize<AccountDto>(cachedData);
            if (cachedDto != null) return cachedDto;
        }

        // Fetch from Postgres
        var account = await _accountRepository.GetByIdAsync(request.AccountId);
        if (account == null)
        {
            throw new DomainException("Account not found.", "ACCOUNT_NOT_FOUND");
        }

        var dto = new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            Status = account.Status
        };

        // Cache in Redis for 10 minutes
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        var serializedData = JsonSerializer.Serialize(dto);
        await _cache.SetStringAsync(cacheKey, serializedData, cacheOptions, cancellationToken);

        return dto;
    }
}
