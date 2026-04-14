using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Qubitlab.Abstractions.Security;
using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Persistence.EFCore.Context;

public abstract class QubitlabDbContext<TContext> : DbContext
    where TContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;
    protected QubitlabDbContext(
        DbContextOptions<TContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options)
    {
        _currentUserService = currentUserService;
    }
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)
                || entityType.ClrType == typeof(AuditLog))
                continue;

            var param  = Expression.Parameter(entityType.ClrType, "e");
            var prop   = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            entityType.SetQueryFilter(filter);
        }
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("__AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.TableName).HasMaxLength(200);
            b.Property(x => x.UserId).HasMaxLength(100);
        });
    }
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ApplyAuditFields();
        return base.SaveChangesAsync(ct);
    }
    public override int SaveChanges()
    {
        ApplyAuditFields();
        return base.SaveChanges();
    }
    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        var currentUser = _currentUserService?.IsAuthenticated == true
            ? _currentUserService.UserId ?? "System"
            : "System";

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUser;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted
                    when entry.Entity is ISoftDeletable sd:
                    entry.State            = EntityState.Modified;
                    sd.IsDeleted           = true;
                    sd.DeletedTime         = now;
                    sd.DeletedBy           = currentUser;
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    break;
            }
        }
    }
}