using System.Linq.Expressions;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace Qubitlab.Persistence.EFCore.Repositories;

public interface IAsyncRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
    
    Task<Paginate<TEntity>> GetPaginateAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int index = 0,
        int size = 10,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
    
    Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
    
    Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
    
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default);
    
    Task<TEntity> SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<ICollection<TEntity>> SoftDeleteRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);

    Task<TEntity> RestoreAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<ICollection<TEntity>> RestoreRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
    
    
    Task<TEntity?> GetAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<List<TEntity>> GetListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
    
  
    Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);
    

    Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default);

    Task<Paginate<TEntity>> GetPaginateAsync(
        ISpecification<TEntity> specification,
        int index = 0,
        int size = 10,
        CancellationToken cancellationToken = default);
}