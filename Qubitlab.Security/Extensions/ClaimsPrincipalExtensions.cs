using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Qubitlab.Security.Extensions;

/// <summary>
/// <see cref="ClaimsPrincipal"/> üzerinde claim okuma kolaylıkları.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>NameIdentifier (sub) claim'inden kullanıcı ID'sini döner.</summary>
    public static string? GetUserId(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
           ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    /// <summary>Email claim'ini döner.</summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Email)?.Value
           ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

    /// <summary>Name claim'inden kullanıcı adını döner.</summary>
    public static string? GetUsername(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Name)?.Value
           ?? principal.FindFirst(JwtRegisteredClaimNames.Name)?.Value;

    /// <summary>JWT ID (jti) claim'ini döner. Token revocation için kullanılır.</summary>
    public static string? GetJti(this ClaimsPrincipal principal)
        => principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

    /// <summary>Tüm rol claim'lerini döner.</summary>
    public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        => principal.FindAll(ClaimTypes.Role).Select(c => c.Value);

    /// <summary>Belirtilen rolü içerip içermediğini kontrol eder.</summary>
    public static bool HasRole(this ClaimsPrincipal principal, string role)
        => principal.IsInRole(role);

    /// <summary>Custom claim'i key ile döner.</summary>
    public static string? GetClaim(this ClaimsPrincipal principal, string claimType)
        => principal.FindFirst(claimType)?.Value;
}
