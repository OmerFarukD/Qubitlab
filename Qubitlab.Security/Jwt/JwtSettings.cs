namespace Qubitlab.Security.Jwt;

/// <summary>
/// JWT ayarları — appsettings.json'dan <c>"Jwt"</c> section'ı ile bind edilir.
/// </summary>
/// <example>
/// appsettings.json:
/// <code>
/// {
///   "Jwt": {
///     "SecretKey": "min-32-char-super-secret-key!!!!!",
///     "Issuer":    "MyApp",
///     "Audience":  "MyAppClients",
///     "AccessTokenExpirationMinutes":  15,
///     "RefreshTokenExpirationDays":     7
///   }
/// }
/// </code>
/// </example>
public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// HMAC-SHA256 imzalama anahtarı. Minimum 32 karakter olmalı.
    /// Production'da environment variable veya secret manager kullan.
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Token'ı oluşturan taraf. Genellikle API adı.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Token'ın hedef kitlesi. Genellikle client adı.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access token geçerlilik süresi (dakika). Varsayılan: 15.</summary>
    public int AccessTokenExpirationMinutes { get; set; } = 15;

    /// <summary>Refresh token geçerlilik süresi (gün). Varsayılan: 7.</summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    // ── Computed helpers ───────────────────────────────
    public TimeSpan AccessTokenExpiration  => TimeSpan.FromMinutes(AccessTokenExpirationMinutes);
    public TimeSpan RefreshTokenExpiration => TimeSpan.FromDays(RefreshTokenExpirationDays);
}
