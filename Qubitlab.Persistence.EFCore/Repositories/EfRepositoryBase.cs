using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Extensions;
using Qubitlab.Persistence.EFCore.Specifications;

namespace Qubitlab.Persistence.EFCore.Repositories;

public class EfRepositoryBase<TEntity, TId, TContext> : IAsyncRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TContext : DbContext
{
    protected readonly TContext Context;
    private readonly bool _autoSaveChanges;

    public EfRepositoryBase(TContext context, bool autoSaveChanges = true)
    {
        Context = context;
        _autoSaveChanges = autoSaveChanges;
    }
    

    public TEntity Add(TEntity entity)
    {
        Context.Entry(entity).State = EntityState.Added;
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entity;
    }

    public ICollection<TEntity> AddRange(ICollection<TEntity> entities)
    {
        Context.AddRange(entities);
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entities;
    }

    public TEntity Update(TEntity entity)
    {
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entity;
    }

    public TEntity Delete(TEntity entity)
    {
        Context.Entry(entity).State = EntityState.Deleted;
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entity;
    }

    public ICollection<TEntity> DeleteRange(ICollection<TEntity> entities)
    {
        Context.RemoveRange(entities);
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entities;
    }

    public int SaveChanges()
    {
        return Context.SaveChanges();
    }

    public TEntity? GetById(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
            
        return queryable.FirstOrDefault(e => e.Id!.Equals(id));
    }

    public Paginate<TEntity> GetPaginate(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int index = 0,
        int size = 10,
        bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
        
        if (predicate != null)
            queryable = queryable.Where(predicate);

        if (orderBy != null)
            return orderBy(queryable).ToPaginate(index, size);

        return queryable.ToPaginate(index, size);
    }

    public List<TEntity> GetList(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
        
        if (predicate != null)
            queryable = queryable.Where(predicate);
        
        if (orderBy != null)
            queryable = orderBy(queryable);

        return queryable.ToList();
    }

    public TEntity? Get(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
            
        return queryable.FirstOrDefault(predicate);
    }

    public bool Any(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = true)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (predicate != null)
            queryable = queryable.Where(predicate);
            
        return queryable.Any();
    }

    public TEntity SoftDelete(TEntity entity)
    {
        entity.IsDeleted = true;
        entity.DeletedTime = DateTime.UtcNow;
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entity;
    }

    public ICollection<TEntity> SoftDeleteRange(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedTime = DateTime.UtcNow;
            Context.Entry(entity).State = EntityState.Modified;
        }
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entities;
    }

    public TEntity Restore(TEntity entity)
    {
        entity.IsDeleted = false;
        entity.DeletedTime = null;
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entity;
    }

    public ICollection<TEntity> RestoreRange(ICollection<TEntity> entities)
    {
        foreach (TEntity entity in entities)
        {
            entity.IsDeleted = false;
            entity.DeletedTime = null;
            Context.Entry(entity).State = EntityState.Modified;
        }
        
        if (_autoSaveChanges)
            Context.SaveChanges();
        
        return entities;
    }

    public TEntity? Get(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).FirstOrDefault();
    }

  
    public List<TEntity> GetList(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).ToList();
    }

    public int Count(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).Count();
    }

    public bool Any(ISpecification<TEntity> specification)
    {
        return ApplySpecification(specification).Any();
    }
    public Paginate<TEntity> GetPaginate(
        ISpecification<TEntity> specification,
        int index = 0,
        int size = 10)
    {
        var query = ApplySpecification(specification);
        return query.ToPaginate(index, size);
    }


    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).State = EntityState.Added;
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await Context.AddRangeAsync(entities, cancellationToken);
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entities;
    }

    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<TEntity> DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Context.Entry(entity).State = EntityState.Deleted;
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<ICollection<TEntity>> DeleteRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        Context.RemoveRange(entities);
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entities;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity?> GetByIdAsync(
        TId id,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
            
        return await queryable.FirstOrDefaultAsync(e => e.Id!.Equals(id), cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetPaginateAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        int index = 0,
        int size = 10,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
        
        if (predicate != null)
            queryable = queryable.Where(predicate);

        if (orderBy != null)
            return await orderBy(queryable).ToPaginateAsync(index, size, cancellationToken);

        return await queryable.ToPaginateAsync(index, size, cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
        
        if (predicate != null)
            queryable = queryable.Where(predicate);
        
        if (orderBy != null)
            queryable = orderBy(queryable);

        return await queryable.ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IQueryable<TEntity>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (include != null)
            queryable = include(queryable);
            
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> queryable = Query();
        
        if (!enableTracking)
            queryable = queryable.AsNoTracking();
        
        if (predicate != null)
            queryable = queryable.Where(predicate);
            
        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<TEntity> SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = true;
        entity.DeletedTime = DateTime.UtcNow;
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<ICollection<TEntity>> SoftDeleteRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (TEntity entity in entities)
        {
            entity.IsDeleted = true;
            entity.DeletedTime = DateTime.UtcNow;
            Context.Entry(entity).State = EntityState.Modified;
        }
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entities;
    }

    public async Task<TEntity> RestoreAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.IsDeleted = false;
        entity.DeletedTime = null;
        Context.Entry(entity).State = EntityState.Modified;
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entity;
    }

    public async Task<ICollection<TEntity>> RestoreRangeAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default)
    {
        foreach (TEntity entity in entities)
        {
            entity.IsDeleted = false;
            entity.DeletedTime = null;
            Context.Entry(entity).State = EntityState.Modified;
        }
        
        if (_autoSaveChanges)
            await Context.SaveChangesAsync(cancellationToken);
        
        return entities;
    }

    public async Task<TEntity?> GetAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<TEntity>> GetListAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).CountAsync(cancellationToken);
    }

    public async Task<bool> AnyAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        return await ApplySpecification(specification).AnyAsync(cancellationToken);
    }

    public async Task<Paginate<TEntity>> GetPaginateAsync(
        ISpecification<TEntity> specification,
        int index = 0,
        int size = 10,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToPaginateAsync(index, size, cancellationToken);
    }


    public IQueryable<TEntity> Query() => Context.Set<TEntity>();
    
    private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
    {
        return SpecificationEvaluator.GetQuery(Query(), specification);
    }
}