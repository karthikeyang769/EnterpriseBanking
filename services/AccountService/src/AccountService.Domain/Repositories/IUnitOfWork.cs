using System.Threading;
using System.Threading.Tasks;

namespace AccountService.Domain.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
