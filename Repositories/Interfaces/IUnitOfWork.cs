using Billing_Software_Api.Models;

namespace Billing_Software_Api.Repositories.Interfaces;

/// <summary>
/// Unit of Work interface managing transactions and repository access across business operations.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
