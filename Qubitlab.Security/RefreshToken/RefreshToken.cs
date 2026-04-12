namespace Qubitlab.Security.RefreshToken;

/// <summary>
/// Refresh token POCO modeli.
/// Storage-agnostic — EF Core, MongoDB veya Redis ile kullanılabilir.
/// </summary>
public sealed class RefreshToken
{
    /// <summary>Benzersiz token string'i (kriptografik random).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Bu token'ın ait olduğu kullanıcının ID'si.</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>Token'ın oluşturulma zamanı (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Token'ın sona erme zamanı (UTC).</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>Token iptal edildi mi?</summary>
    public bool IsRevoked { get; set; } = false;

    /// <summary>Token kullanıldı mı? (rotation için)</summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>Bu token hangi access token ile birlikte üretildi? (jti)</summary>
    public string? AccessTokenJti { get; set; }

    /// <summary>Token'ın süresi doldu mu veya iptal edildi mi?</summary>
    public bool IsExpiredOrRevoked => IsRevoked || IsUsed || DateTime.UtcNow > ExpiresAt;
}
