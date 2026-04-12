
namespace Qubitlab.Persistence.EFCore.Entities;

public abstract class Entity<TId> : IAuditableEntity, ISoftDeletable
{
    public TId Id { get; set; } = default!;

    // IAuditableEntity
    public DateTime CreatedAt  { get; set; }
    public string?  CreatedBy  { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string?  UpdatedBy  { get; set; }

    // Repo convenience aliases (UTC timestamp'ler)
    public DateTime  CreatedTime { get; set; }
    public DateTime? UpdatedTime { get; set; }

    // ISoftDeletable
    public bool      IsDeleted   { get; set; }
    public DateTime? DeletedTime { get; set; }
    public string?   DeletedBy   { get; set; }

    protected Entity()
    {
        IsDeleted = false;
    }

    protected Entity(TId id)
    {
        Id        = id;
        IsDeleted = false;
    }
}
