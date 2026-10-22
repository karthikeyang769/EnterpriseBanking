using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using AccountService.Domain.Repositories;
using AccountService.Application.DTOs;

namespace AccountService.Application.Queries;

public record GetAccountsByCustomerIdQuery(Guid CustomerId) : IRequest<IEnumerable<AccountDto>>;

public class GetAccountsByCustomerIdQueryHandler : IRequestHandler<GetAccountsByCustomerIdQuery, IEnumerable<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;

    public GetAccountsByCustomerIdQueryHandler(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<IEnumerable<AccountDto>> Handle(GetAccountsByCustomerIdQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.GetByCustomerIdAsync(request.CustomerId);
        return accounts.Select(account => new AccountDto
        {
            Id = account.Id,
            CustomerId = account.CustomerId,
            AccountNumber = account.AccountNumber,
            AccountType = account.AccountType,
            Balance = account.Balance,
            Status = account.Status
        }).ToList();
    }
}
