using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Qubitlab.Abstractions.Security;

namespace Qubitlab.Persistence.EFCore.Services;

public class DefaultCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DefaultCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User?.FindFirst("sub")?.Value
        ?? User?.FindFirst("uid")?.Value;

    public string? Username =>
        User?.FindFirst(ClaimTypes.Name)?.Value
        ?? User?.FindFirst("preferred_username")?.Value
        ?? User?.Identity?.Name;

    public string? Email =>
        User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("email")?.Value;

    public IReadOnlyList<string> Roles =>
        User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList().AsReadOnly()
        ?? [];

    public bool IsInRole(string role) =>
        User?.IsInRole(role) ?? false;

    public string? GetClaim(string claimType) =>
        User?.FindFirst(claimType)?.Value;
}
