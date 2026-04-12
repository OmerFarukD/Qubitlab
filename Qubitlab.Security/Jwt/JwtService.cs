using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Qubitlab.Security.Jwt;

/// <summary>
/// HMAC-SHA256 imzalı JWT token üretimi ve doğrulaması.
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    private readonly JwtSecurityTokenHandler _handler = new();

    public JwtService(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;

        if (string.IsNullOrWhiteSpace(_settings.SecretKey) || _settings.SecretKey.Length < 32)
            throw new InvalidOperationException(
                "Jwt:SecretKey en az 32 karakter uzunluğunda olmalıdır.");
    }

    /// <inheritdoc />
    public AccessToken CreateToken(IEnumerable<Claim> claims)
    {
        var claimList = claims.ToList();

        // Her token için benzersiz jti (JWT ID) — blacklist için kullanılır
        if (!claimList.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
            claimList.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

        var expiresAt   = DateTime.UtcNow.Add(_settings.AccessTokenExpiration);
        var signingKey  = BuildSigningKey();

        var descriptor = new SecurityTokenDescriptor
        {
            Subject            = new ClaimsIdentity(claimList),
            Expires            = expiresAt,
            Issuer             = _settings.Issuer,
            Audience           = _settings.Audience,
            SigningCredentials  = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
        };

        var token = _handler.CreateToken(descriptor);
        var tokenString = _handler.WriteToken(token);

        return new AccessToken(tokenString, expiresAt, claimList.AsReadOnly());
    }

    /// <inheritdoc />
    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            return _handler.ValidateToken(token, BuildValidationParameters(), out _);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        // Süre doğrulamasını kapat — sadece imza ve yapıyı kontrol et
        var parameters = BuildValidationParameters();
        parameters.ValidateLifetime = false;

        try
        {
            return _handler.ValidateToken(token, parameters, out _);
        }
        catch
        {
            return null;
        }
    }

    // ────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────

    private SymmetricSecurityKey BuildSigningKey() =>
        new(Encoding.UTF8.GetBytes(_settings.SecretKey));

    private TokenValidationParameters BuildValidationParameters() =>
        new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey         = BuildSigningKey(),
            ValidateIssuer           = !string.IsNullOrWhiteSpace(_settings.Issuer),
            ValidIssuer              = _settings.Issuer,
            ValidateAudience         = !string.IsNullOrWhiteSpace(_settings.Audience),
            ValidAudience            = _settings.Audience,
            ValidateLifetime         = true,
            ClockSkew                = TimeSpan.Zero   // süresi bittiğinde anında geçersiz
        };
}
