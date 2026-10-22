using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AccountService.Domain.Entities;

namespace AccountService.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    Task<IEnumerable<Account>> GetByCustomerIdAsync(Guid customerId);
    Task AddAsync(Account account);
    void Update(Account account);
}
