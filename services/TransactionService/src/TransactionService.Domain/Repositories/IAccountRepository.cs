using System.Threading.Tasks;
using TransactionService.Domain.Entities;

namespace TransactionService.Domain.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByAccountNumberAsync(string accountNumber);
    void Update(Account account);
}
