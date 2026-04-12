namespace Qubitlab.Security.RefreshToken;

/// <summary>
/// Refresh token iş mantığı kontratı.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Yeni refresh token üretir ve storage'a kaydeder.</summary>
    Task<RefreshToken> CreateAsync(string userId, string? accessTokenJti = null, CancellationToken ct = default);

    /// <summary>Token string'i ile token'ı getirir.</summary>
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Eski token'ı geçersiz kılar ve yerine yeni token üretir.
    /// Refresh token rotation — token çalınırsa zararı sınırlar.
    /// </summary>
    Task<RefreshToken> RotateAsync(string oldToken, string userId, string? newAccessTokenJti = null, CancellationToken ct = default);

    /// <summary>Tek bir token'ı revoke eder.</summary>
    Task RevokeAsync(string token, CancellationToken ct = default);

    /// <summary>Kullanıcının tüm token'larını revoke eder (logout everywhere).</summary>
    Task RevokeAllAsync(string userId, CancellationToken ct = default);
}
