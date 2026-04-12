using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Security.RefreshToken;

namespace Qubitlab.Identity.Entities;

/// <summary>
/// Refresh token'ın veritabanı entity karşılığı.
/// <see cref="Entity{TId}"/> üzerinden audit otomatik yönetilir.
/// <see cref="RefreshToken"/> POCO modeli ile aynı verileri taşır — EF Core için kalıtım.
/// </summary>
public class IdentityRefreshToken : Entity<Guid>
{
    /// <summary>Kriptografik güvenli token string'i.</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Bu token'ın ait olduğu kullanıcı.</summary>
    public Guid UserId { get; set; }

    /// <summary>Token'ın sona erme zamanı (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Token iptal edildi mi?</summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>Token kullanıldı mı? (rotation)</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Bu token ile birlikte üretilen access token'ın jti'si.</summary>
    public string? AccessTokenJti { get; set; }

    /// <summary>Token'ın süresi doldu mu veya iptal edildi mi?</summary>
    public bool IsExpiredOrRevoked => IsRevoked || IsUsed || DateTime.UtcNow > ExpiresAt;

    // ─── Navigation ───────────────────────────────────
    public QubitlabUser User { get; set; } = null!;

    // ─── Factory ──────────────────────────────────────
    /// <summary>
    /// Security katmanındaki POCO <see cref="RefreshToken"/>'dan
    /// entity oluşturmak için factory metod.
    /// </summary>
    public static IdentityRefreshToken FromPoco(RefreshToken poco, Guid userId) => new()
    {
        Id             = Guid.NewGuid(),
        Token          = poco.Token,
        UserId         = userId,
        ExpiresAt      = poco.ExpiresAt,
        IsRevoked      = poco.IsRevoked,
        IsUsed         = poco.IsUsed,
        AccessTokenJti = poco.AccessTokenJti
    };

    /// <summary>Entity'yi POCO modeline dönüştürür.</summary>
    public RefreshToken ToPoco() => new()
    {
        Token          = Token,
        UserId         = UserId.ToString(),
        CreatedAt      = CreatedAt,
        ExpiresAt      = ExpiresAt,
        IsRevoked      = IsRevoked,
        IsUsed         = IsUsed,
        AccessTokenJti = AccessTokenJti
    };
}
