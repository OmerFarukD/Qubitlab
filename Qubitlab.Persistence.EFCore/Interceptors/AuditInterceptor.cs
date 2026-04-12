using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Services;

namespace Qubitlab.Persistence.EFCore.Interceptors;

public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateAuditFields(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateAuditFields(DbContext? context)
    {
        if (context == null) return;

        var currentUser = GetCurrentUser();
        var entries = context.ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // Yeni kayıt eklenirken
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
               
                    entry.Entity.UpdatedBy = currentUser;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    
         
                    entry.Property(nameof(IAuditableEntity.CreatedBy)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.CreatedAt)).IsModified = false;
                    break;

                case EntityState.Deleted:
              
                    if (entry.Entity is ISoftDeletable softDeletable)
                    {
                        entry.State = EntityState.Modified;
                        softDeletable.IsDeleted = true;
                        softDeletable.DeletedTime = DateTime.UtcNow;
                        softDeletable.DeletedBy = currentUser;
                        
                        entry.Entity.UpdatedBy = currentUser;
                        entry.Entity.UpdatedAt = DateTime.UtcNow;
                    }
                    break;
            }
        }
    }
    private string GetCurrentUser()
    {
        if (_currentUserService.IsAuthenticated)
        {
            return _currentUserService.UserId 
                   ?? _currentUserService.Username 
                   ?? "System";
        }

        return "System";
    }
}