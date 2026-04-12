namespace Qubitlab.Security.RefreshToken;

/// <summary>
/// Refresh token storage kontratı.
/// Tüketici proje kendi storage'ına göre implement eder
/// (EF Core, MongoDB, Redis vb.)
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>Token string'i ile token'ı getirir. Yoksa <c>null</c> döner.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Kullanıcıya ait aktif token'ları listeler.</summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>Yeni refresh token kaydeder.</summary>
    Task AddAsync(RefreshToken token, CancellationToken ct = default);

    /// <summary>Token'ı revoke eder (IsRevoked = true).</summary>
    Task RevokeAsync(string token, CancellationToken ct = default);

    /// <summary>Kullanıcıya ait tüm token'ları revoke eder (logout everywhere).</summary>
    Task RevokeAllByUserIdAsync(string userId, CancellationToken ct = default);

    /// <summary>Süresi dolmuş token'ları temizler (opsiyonel — background job için).</summary>
    Task DeleteExpiredAsync(CancellationToken ct = default);
}
