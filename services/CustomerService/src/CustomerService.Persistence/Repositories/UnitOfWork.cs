using System.Threading;
using System.Threading.Tasks;
using CustomerService.Domain.Repositories;

namespace CustomerService.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CustomerDbContext _context;

    public UnitOfWork(CustomerDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
