using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Persistence.EFCore.Extensions;

public static class ModelBuilderExtensions
{
    public static ModelBuilder ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(et => et.ClrType.IsAssignableToGenericType(typeof(Entity<>)))
            .Select(et => et.ClrType);

        foreach (var entityType in entityTypes)
        {
            var parameter = Expression.Parameter(entityType, "e");
            var property = Expression.Property(parameter, nameof(Entity<object>.IsDeleted));
            var body = Expression.Equal(property, Expression.Constant(false));
            var lambda = Expression.Lambda(body, parameter);

            modelBuilder.Entity(entityType).HasQueryFilter(lambda);
        }

        return modelBuilder;
    }
    
    public static ModelBuilder ConfigureAuditIndexes(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IAuditableEntity.CreatedBy))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_CreatedBy");

                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(IAuditableEntity.CreatedAt))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_CreatedAt");
            }

            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .HasIndex(nameof(ISoftDeletable.IsDeleted))
                    .HasDatabaseName($"IX_{entityType.GetTableName()}_IsDeleted");
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// AuditLog için konfigürasyon
    /// </summary>
    public static ModelBuilder ConfigureAuditLog(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(e => e.TableName);
            entity.HasIndex(e => e.RecordId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TableName, e.RecordId });
        });

        return modelBuilder;
    }
    
    public static ModelBuilder ApplySoftDeleteQueryFilter<TEntity>(
        this ModelBuilder modelBuilder,
        Expression<Func<TEntity, bool>> filter) where TEntity : class
    {
        modelBuilder.Entity<TEntity>().HasQueryFilter(filter);
        return modelBuilder;
    }
    
    private static bool IsAssignableToGenericType(this Type type, Type genericType)
    {
        while (type != null && type != typeof(object))
        {
            var current = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            if (genericType == current)
                return true;

            type = type.BaseType!;
        }
        return false;
    }
}