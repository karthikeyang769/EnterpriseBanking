using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TransactionService.Domain.Entities;
using TransactionService.Domain.Repositories;

namespace TransactionService.Persistence.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly TransactionDbContext _context;

    public AccountRepository(TransactionDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByAccountNumberAsync(string accountNumber)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public void Update(Account account)
    {
        _context.Accounts.Update(account);
    }
}
