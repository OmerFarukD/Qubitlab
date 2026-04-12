using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Qubitlab.Security.Hashing;
using Qubitlab.Security.Jwt;
using Qubitlab.Security.RefreshToken;
using Qubitlab.Security.Revocation;

namespace Qubitlab.Security.Extensions;

/// <summary>
/// Qubitlab.Security paketini DI container'a kayıt eden extension metodlar.
/// </summary>
public static class SecurityServiceExtensions
{
    /// <summary>
    /// JWT, hashing ve token revocation servislerini kayıt eder.
    /// </summary>
    /// <param name="services">DI konteyneri</param>
    /// <param name="configureJwt">JWT ayarlarını konfigüre eden Action</param>
    /// <param name="algorithm">Şifre hashleme algoritması (varsayılan: BCrypt)</param>
    /// <param name="enableRevocation">Token blacklist aktif mi? (varsayılan: true)</param>
    /// <example>
    /// <code>
    /// builder.Services.AddQubitlabSecurity(
    ///     configureJwt: opt =>
    ///     {
    ///         opt.SecretKey                 = builder.Configuration["Jwt:SecretKey"]!;
    ///         opt.Issuer                    = "MyApp";
    ///         opt.Audience                  = "MyAppClients";
    ///         opt.AccessTokenExpirationMinutes  = 15;
    ///         opt.RefreshTokenExpirationDays    = 7;
    ///     },
    ///     algorithm:        HashingAlgorithm.BCrypt,
    ///     enableRevocation: true);
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabSecurity(
        this IServiceCollection services,
        Action<JwtSettings> configureJwt,
        HashingAlgorithm algorithm = HashingAlgorithm.BCrypt,
        bool enableRevocation = true)
    {
        ArgumentNullException.ThrowIfNull(configureJwt);

        // ── JWT Settings ───────────────────────────────────
        services.Configure(configureJwt);
        var jwtSettings = new JwtSettings();
        configureJwt(jwtSettings);

        // ── JWT Service ────────────────────────────────────
        services.AddSingleton<IJwtService, JwtService>();

        // ── JWT Bearer Authentication ──────────────────────
        services
            .AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer   = !string.IsNullOrWhiteSpace(jwtSettings.Issuer),
                    ValidIssuer      = jwtSettings.Issuer,
                    ValidateAudience = !string.IsNullOrWhiteSpace(jwtSettings.Audience),
                    ValidAudience    = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew        = TimeSpan.Zero
                };
            });

        // ── Hashing ───────────────────────────────────────
        services.AddSingleton<IHashingService>(algorithm switch
        {
            HashingAlgorithm.BCrypt => new BCryptHashingService(),
            HashingAlgorithm.Pbkdf2 => new Pbkdf2HashingService(),
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm))
        });

        // ── Refresh Token ──────────────────────────────────
        // IRefreshTokenRepository tüketici proje tarafından register edilmeli
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();

        // ── Token Revocation ───────────────────────────────
        if (enableRevocation)
        {
            // ICacheService'in DI'a kayıtlı olması gerekir
            // (AddQubitlabInMemoryCache / AddQubitlabRedisCache / AddQubitlabHybridCache)
            services.AddSingleton<ITokenRevocationService, CacheTokenRevocationService>();
        }

        return services;
    }
}
