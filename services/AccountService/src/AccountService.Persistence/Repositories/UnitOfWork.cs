using System.Threading;
using System.Threading.Tasks;
using AccountService.Domain.Repositories;

namespace AccountService.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AccountDbContext _context;

    public UnitOfWork(AccountDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
