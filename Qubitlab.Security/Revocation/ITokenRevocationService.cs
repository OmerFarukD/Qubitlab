using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Security.Revocation;

/// <summary>
/// JWT token revocation (blacklist) kontratı.
/// Logout olan kullanıcının hâlâ geçerli access token'larını bloklamak için kullanılır.
/// </summary>
public interface ITokenRevocationService
{
    /// <summary>
    /// Token'ı (jti claim) blacklist'e ekler.
    /// </summary>
    /// <param name="jti">JWT ID claim değeri (jti)</param>
    /// <param name="tokenRemainingLifetime">Token'ın kalan geçerlilik süresi — bu süre kadar cache'de tutulur</param>
    /// <param name="ct">İptal token'ı</param>
    Task RevokeAsync(string jti, TimeSpan tokenRemainingLifetime, CancellationToken ct = default);

    /// <summary>Token'ın revoke edilip edilmediğini kontrol eder.</summary>
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}
