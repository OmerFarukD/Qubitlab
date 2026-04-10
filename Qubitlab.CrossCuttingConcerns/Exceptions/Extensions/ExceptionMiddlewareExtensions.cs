using Microsoft.AspNetCore.Builder;
using Qubitlab.CrossCuttingConcerns.Exceptions.Middlewares;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseQubitlabExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionMiddleware>();
}
