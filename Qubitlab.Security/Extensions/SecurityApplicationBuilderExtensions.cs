using Microsoft.AspNetCore.Builder;
using Qubitlab.Security.Middleware;

namespace Qubitlab.Security.Extensions;

/// <summary>
/// <see cref="IApplicationBuilder"/> üzerinde Qubitlab Security middleware extension'ları.
/// </summary>
public static class SecurityApplicationBuilderExtensions
{
    /// <summary>
    /// Token revocation (blacklist) middleware'ini pipeline'a ekler.
    /// </summary>
    /// <remarks>
    /// Kullanım sırası önemlidir:
    /// <code>
    /// app.UseAuthentication();          // 1. önce JWT doğrula
    /// app.UseQubitlabTokenRevocation(); // 2. sonra blacklist kontrol et
    /// app.UseAuthorization();           // 3. en son yetkilendirme
    /// </code>
    /// </remarks>
    public static IApplicationBuilder UseQubitlabTokenRevocation(
        this IApplicationBuilder app)
        => app.UseMiddleware<TokenRevocationMiddleware>();
}
