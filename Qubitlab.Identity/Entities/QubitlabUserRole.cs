namespace Qubitlab.Identity.Entities;

/// <summary>
/// Kullanıcı-Rol many-to-many ilişki tablosu.
/// EF Core join entity — payload (AssignedAt) içerdiği için ayrı class.
/// </summary>
public class QubitlabUserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }

    /// <summary>Rolün kullanıcıya atandığı zaman.</summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Rolü atayan kullanıcı/sistem (opsiyonel).</summary>
    public string? AssignedBy { get; set; }

    // ─── Navigation ───────────────────────────────────
    public QubitlabUser User { get; set; } = null!;
    public QubitlabRole Role { get; set; } = null!;
}
