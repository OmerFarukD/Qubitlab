using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;       // GetTableName() — RelationalEntityTypeExtensions

using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Services;

namespace Qubitlab.Persistence.EFCore.Interceptors;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogInterceptor(
        ICurrentUserService currentUserService,
        IHttpContextAccessor httpContextAccessor)
    {
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CreateAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CreateAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void CreateAuditLogs(DbContext? context)
    {
        if (context == null) return;

        var auditLogs = new List<AuditLog>();
        var entries = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditLog && 
                        (e.State == EntityState.Added ||
                         e.State == EntityState.Modified ||
                         e.State == EntityState.Deleted))
            .ToList();

        foreach (var entry in entries)
        {
            var auditLog = CreateAuditLog(entry);
            if (auditLog != null)
            {
                auditLogs.Add(auditLog);
            }
        }

        if (auditLogs.Any())
        {
            context.Set<AuditLog>().AddRange(auditLogs);
        }
    }

    private AuditLog? CreateAuditLog(EntityEntry entry)
    {
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
            RecordId = GetPrimaryKeyValue(entry),
            Action = entry.State.ToString(),
            UserId = _currentUserService.UserId ?? "System",
            Timestamp = DateTime.UtcNow,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent()
        };

        var changes = new Dictionary<string, object?>();
        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        switch (entry.State)
        {
            case EntityState.Added:
                foreach (var property in entry.Properties)
                {
                    if (ShouldAudit(property))
                    {
                        newValues[property.Metadata.Name] = property.CurrentValue;
                        changes[property.Metadata.Name] = property.CurrentValue;
                    }
                }
                break;

            case EntityState.Modified:
                foreach (var property in entry.Properties)
                {
                    if (ShouldAudit(property) && property.IsModified)
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        newValues[property.Metadata.Name] = property.CurrentValue;
                        changes[property.Metadata.Name] = new
                        {
                            Old = property.OriginalValue,
                            New = property.CurrentValue
                        };
                    }
                }
                break;

            case EntityState.Deleted:
                foreach (var property in entry.Properties)
                {
                    if (ShouldAudit(property))
                    {
                        oldValues[property.Metadata.Name] = property.OriginalValue;
                        changes[property.Metadata.Name] = property.OriginalValue;
                    }
                }
                break;
        }

        if (!changes.Any())
            return null;

        auditLog.Changes = JsonSerializer.Serialize(changes);
        auditLog.OldValues = oldValues.Any() ? JsonSerializer.Serialize(oldValues) : null;
        auditLog.NewValues = newValues.Any() ? JsonSerializer.Serialize(newValues) : null;

        return auditLog;
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyValues = entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString())
            .ToList();

        return string.Join("_", keyValues);
    }

    private static bool ShouldAudit(PropertyEntry property)
    {

        // Audit timestamp alanları kayıt dışı tutulur
        var excludedProperties = new[]
        {
            "CreatedBy",
            "CreatedAt",
            "UpdatedBy",
            "UpdatedAt",
            "CreatedTime",
            "UpdatedTime"
        };

        return !excludedProperties.Contains(property.Metadata.Name);
    }

    private string? GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();
    }
}