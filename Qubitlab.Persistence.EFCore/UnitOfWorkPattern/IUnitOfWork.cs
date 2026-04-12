using Microsoft.EntityFrameworkCore.Storage;

namespace Qubitlab.Persistence.EFCore.UnitOfWorkPattern;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    IDbContextTransaction? GetCurrentTransaction();
    int SaveChanges();
    IDbContextTransaction BeginTransaction();
    void CommitTransaction();
    void RollbackTransaction();
}