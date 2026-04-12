using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Qubitlab.Security.Extensions;
using Qubitlab.Security.Revocation;

namespace Qubitlab.Security.Middleware;

/// <summary>
/// JWT token revocation middleware.
/// <c>UseAuthentication()</c>'dan SONRA pipeline'a eklenmelidir.
/// </summary>
/// <remarks>
/// Akış:
/// <code>
/// İstek gelir
///   → Kullanıcı authenticate değil → geç (anonymous endpoint)
///   → Kullanıcı authenticate ama JTI claim yok → geç (edge case)
///   → JTI blacklist'te var (logout edilmiş) → 401 döndür
///   → JTI temiz → next() — normal akış devam eder
/// </code>
///
/// Neden <c>UseAuthentication()</c>'dan SONRA?
/// Çünkü token doğrulaması tamamlandıktan sonra
/// <see cref="HttpContext.User"/> claim'leri dolu olur.
/// JTI'yi claim'den okumak, token'ı ikinci kez parse etmekten çok daha hızlıdır.
/// </remarks>
public sealed class TokenRevocationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenRevocationMiddleware> _logger;

    public TokenRevocationMiddleware(
        RequestDelegate next,
        ILogger<TokenRevocationMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITokenRevocationService revocationService)
    {
        // 1. Kullanıcı authenticate değilse kontrol etmeye gerek yok
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // 2. JTI claim'ini oku (ClaimsPrincipalExtensions helper'ı ile)
        var jti = context.User.GetJti();

        if (string.IsNullOrWhiteSpace(jti))
        {
            // JTI yoksa token revocation desteklenmiyor — geç
            await _next(context);
            return;
        }

        // 3. Blacklist kontrolü — ICacheService üzerinden (InMemory/Redis/Hybrid)
        var isRevoked = await revocationService.IsRevokedAsync(jti, context.RequestAborted);

        if (isRevoked)
        {
            _logger.LogWarning(
                "Revoked token used. JTI: {Jti}, IP: {Ip}, Path: {Path}",
                jti,
                context.Connection.RemoteIpAddress,
                context.Request.Path);

            context.Response.StatusCode  = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var body = JsonSerializer.Serialize(new
            {
                type    = "https://tools.ietf.org/html/rfc7235#section-3.1",
                title   = "Unauthorized",
                status  = 401,
                detail  = "Token has been revoked. Please login again.",
                traceId = context.TraceIdentifier
            });

            await context.Response.WriteAsync(body, context.RequestAborted);
            return;
        }

        // 4. Token temiz — devam et
        await _next(context);
    }
}
