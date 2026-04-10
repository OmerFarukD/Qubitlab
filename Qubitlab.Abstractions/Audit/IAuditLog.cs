namespace Qubitlab.Abstractions.Audit;

public interface IAuditLog
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}