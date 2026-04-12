using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Identity.Entities;

/// <summary>
/// Sistemdeki kullanıcıyı temsil eden entity.
/// <see cref="Entity{TId}"/> üzerinden audit ve soft-delete otomatik sağlanır.
/// </summary>
public class QubitlabUser : Entity<Guid>
{
    /// <summary>Kullanıcının tam adı.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Benzersiz e-posta adresi.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Normalize edilmiş e-posta (büyük harf) — indeks için.</summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    /// <summary>
    /// BCrypt / PBKDF2 ile hashlenmiş şifre.
    /// Plain text hiçbir zaman burada tutulmaz.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>E-posta doğrulandı mı?</summary>
    public bool EmailConfirmed { get; set; } = false;

    /// <summary>Telefon numarası (opsiyonel).</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Telefon doğrulandı mı?</summary>
    public bool PhoneNumberConfirmed { get; set; } = false;

    /// <summary>İki faktörlü doğrulama aktif mi?</summary>
    public bool TwoFactorEnabled { get; set; } = false;

    /// <summary>Hesap kilitli mi? (çok fazla başarısız giriş vb.)</summary>
    public bool LockoutEnabled { get; set; } = false;

    /// <summary>Kilit bitiş zamanı. null → kilitli değil.</summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>Başarısız giriş sayısı.</summary>
    public int AccessFailedCount { get; set; } = 0;

    /// <summary>Hesap aktif mi? Soft deactivation için.</summary>
    public bool IsActive { get; set; } = true;

    // ─── Navigation ───────────────────────────────────
    public ICollection<QubitlabUserRole> UserRoles { get; set; } = [];
}
