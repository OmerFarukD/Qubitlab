using MediatR;
using Microsoft.AspNetCore.Http;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Qubitlab.Security.Extensions;

namespace Qubitlab.Application.Pipelines.Authorization;

/// <summary>
/// MediatR pipeline behavior that enforces authorization for requests
/// implementing <see cref="IAuthRequired"/>.
/// </summary>
/// <remarks>
/// Two-stage check:
/// <list type="number">
///   <item>Verifies the user is authenticated (Identity.IsAuthenticated).</item>
///   <item>Verifies the user holds at least one of the roles declared in <see cref="IAuthRequired.Roles"/>.</item>
/// </list>
/// Throws <see cref="AuthorizationException"/> (HTTP 401) when the user is not
/// authenticated, and (HTTP 403) when the user is authenticated but lacks the
/// required roles.
/// </remarks>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthRequired
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Stage 1 — Authentication check (HTTP 401)
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new AuthorizationException("You are not authenticated. Please log in to continue.");
        }

        // Stage 2 — Authorization check: ensure the user has at least one role
        var userRoleClaims = httpContext.User.GetRoles();

        if (userRoleClaims == null || !userRoleClaims.Any())
        {
            // User is authenticated but has no roles assigned — this is an
            // authorization failure, not an authentication failure.
            throw new AuthorizationException("You do not have any roles assigned. Access is denied.");
        }

        // Stage 3 — Role match: check against roles declared on the request (HTTP 403)
        bool isAuthorized = request.Roles.Any(role => userRoleClaims.Contains(role));

        if (!isAuthorized)
        {
            throw new AuthorizationException(
                "You do not have permission to perform this action. Required roles: "
                + string.Join(", ", request.Roles));
        }

        return await next();
    }
}
