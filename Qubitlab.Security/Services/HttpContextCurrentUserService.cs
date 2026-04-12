using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Qubitlab.Abstractions.Security;
using Qubitlab.Security.Extensions;

namespace Qubitlab.Security.Services;

/// <summary>
/// <see cref="ICurrentUserService"/> implementasyonu.
/// Kullanıcı bilgilerini <see cref="IHttpContextAccessor"/> üzerinden JWT claim'lerinden okur.
/// </summary>
/// <remarks>
/// <b>DI Lifetime: Scoped</b> — Her HTTP isteği için ayrı instance.
/// HttpContext request bazlı olduğundan Singleton olmamalı.
/// </remarks>
public sealed class HttpContextCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Lazy evaluation — her property erişiminde HttpContext sorgulanmaz
    private ClaimsPrincipal? Principal =>
        _httpContextAccessor.HttpContext?.User;

    public HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public string? UserId =>
        Principal?.GetUserId();

    /// <inheritdoc />
    public string? Email =>
        Principal?.GetEmail();

    /// <inheritdoc />
    public string? Username =>
        Principal?.GetUsername();

    /// <inheritdoc />
    public IReadOnlyList<string> Roles =>
        Principal?.GetRoles().ToList().AsReadOnly()
        ?? (IReadOnlyList<string>)[];

    /// <inheritdoc />
    public bool IsInRole(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);
        return Principal?.HasRole(role) == true;
    }

    /// <inheritdoc />
    public string? GetClaim(string claimType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(claimType);
        return Principal?.GetClaim(claimType);
    }
}
