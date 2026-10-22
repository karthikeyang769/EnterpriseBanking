using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id);
    Task<Transaction?> GetByReferenceNumberAsync(string referenceNumber);
    Task<IEnumerable<Transaction>> GetByAccountNumberAsync(string accountNumber);
    Task AddAsync(Transaction transaction);
}
