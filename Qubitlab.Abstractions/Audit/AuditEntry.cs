namespace Qubitlab.Abstractions.Audit;

public sealed class AuditEntry
{
    public string EntityName { get; init; } = default!;
    public string Action { get; init; } = default!;
    public string? UserId { get; init; }
    public string? OldValues { get; init; }
    public string? NewValues { get; init; }
    public DateTime Timestamp { get; init; }
    public IDictionary<string, object?> PrimaryKey { get; init; } = new Dictionary<string, object?>();

}