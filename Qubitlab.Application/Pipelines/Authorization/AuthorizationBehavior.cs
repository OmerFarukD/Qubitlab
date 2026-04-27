using MediatR;
using Microsoft.AspNetCore.Http;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Qubitlab.Security.Extensions;

namespace Qubitlab.Application.Pipelines.Authorization;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthRequired
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new AuthorizationException("You are not authenticated.");
        }

        // Kullanıcı rollerini al
        var userRoleClaims = httpContext.User.GetRoles();

        if (userRoleClaims == null || !userRoleClaims.Any())
        {
            throw new AuthorizationException("You are not authenticated.");
        }

    
        bool isAuthorized = userRoleClaims.Contains("Admin") ||
                            request.Roles.Any(role => userRoleClaims.Contains(role));

        if (!isAuthorized)
        {
            throw new AuthorizationException("Yetkiniz Yok");
        }

      
        return await next();
    }

}