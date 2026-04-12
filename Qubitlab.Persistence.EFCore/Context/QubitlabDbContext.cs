using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Services;

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
    // ✅ Problem 1 çözüldü: AuditLog DbSet base'de — kullanıcı
    //    kendi DbContext'ine eklemeye gerek yok
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ✅ Problem 2 çözüldü: Global soft-delete filter otomatik
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // AuditLog tablosuna soft-delete filter uygulanmamalı
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)
                || entityType.ClrType == typeof(AuditLog))
                continue;

            var param  = Expression.Parameter(entityType.ClrType, "e");
            var prop   = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
            var filter = Expression.Lambda(Expression.Not(prop), param);
            entityType.SetQueryFilter(filter);
        }
        // AuditLog tablosunu yapılandır
        modelBuilder.Entity<AuditLog>(b =>
        {
            b.ToTable("__AuditLogs");
            b.HasKey(x => x.Id);
            b.Property(x => x.TableName).HasMaxLength(200);
            b.Property(x => x.UserId).HasMaxLength(100);
        });
    }
    // ✅ Problem 3 çözüldü: Audit alanları SaveChanges'de otomatik dolar
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

        // ✅ Entity<object> değil — IAuditableEntity interface'i ile filtrele
        //    Bu sayede Entity<Guid>, Entity<int> vs. hepsi yakalanır
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
                    // CreatedAt/CreatedBy hiçbir zaman değiştirilemesin
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    break;

                case EntityState.Deleted
                    when entry.Entity is ISoftDeletable sd:
                    // ✅ Hard delete → soft delete dönüşümü
                    entry.State            = EntityState.Modified;
                    sd.IsDeleted           = true;
                    sd.DeletedTime         = now;
                    sd.DeletedBy           = currentUser;
                    // Soft delete de bir "değişiklik" — UpdatedAt'ı da doldur
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUser;
                    break;
            }
        }
    }
}