using System.Security.Claims;

namespace Qubitlab.Security.Jwt;

/// <summary>
/// Başarıyla üretilen JWT access token'ını temsil eder.
/// </summary>
/// <param name="Token">İmzalanmış JWT string'i — Authorization: Bearer {Token}</param>
/// <param name="ExpiresAt">Token'ın UTC sona erme zamanı</param>
/// <param name="Claims">Token içindeki claim'lerin listesi</param>
public sealed record AccessToken(
    string Token,
    DateTime ExpiresAt,
    IReadOnlyList<Claim> Claims
);
