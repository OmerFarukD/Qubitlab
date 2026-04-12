namespace Qubitlab.Persistence.EFCore.Entities;

public class AuditLog : Entity<Guid>
{
    public string TableName { get; set; } = string.Empty;
    

    public string RecordId { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
    
  
    public string Changes { get; set; } = string.Empty;
    

    public string? OldValues { get; set; }
    
 
    public string? NewValues { get; set; }
    

    public string? UserId { get; set; }

    public DateTime Timestamp { get; set; }

    public string? IpAddress { get; set; }
    

    public string? UserAgent { get; set; }
}