using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Identity.Entities;

/// <summary>
/// Yetki rolünü temsil eden entity.
/// Örnek: "Admin", "User", "Moderator"
/// </summary>
public class QubitlabRole : Entity<Guid>
{
    /// <summary>Rol adı. Örn: "Admin", "User"</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Normalize edilmiş rol adı (büyük harf) — indeks ve karşılaştırma için.</summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>Rol hakkında açıklama (opsiyonel).</summary>
    public string? Description { get; set; }

    // ─── Navigation ───────────────────────────────────
    public ICollection<QubitlabUserRole> UserRoles { get; set; } = [];
}
