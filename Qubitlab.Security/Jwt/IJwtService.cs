using System.Security.Claims;

namespace Qubitlab.Security.Jwt;

/// <summary>
/// JWT token üretme ve doğrulama kontratı.
/// </summary>
public interface IJwtService
{
    /// <summary>
    /// Verilen claim'lerden imzalı bir JWT access token üretir.
    /// </summary>
    /// <param name="claims">Token'a gömülecek claim'ler (UserId, Email, Role vb.)</param>
    AccessToken CreateToken(IEnumerable<Claim> claims);

    /// <summary>
    /// Token'ı doğrular. Geçerliyse <see cref="ClaimsPrincipal"/> döner, geçersizse <c>null</c>.
    /// </summary>
    /// <param name="token">Doğrulanacak JWT string'i</param>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Süresi dolmuş olsa bile token'dan <see cref="ClaimsPrincipal"/> çıkarır.
    /// Refresh token akışında, süresi dolmuş access token'ı çözmek için kullanılır.
    /// </summary>
    /// <param name="token">Süresi dolmuş JWT string'i</param>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
