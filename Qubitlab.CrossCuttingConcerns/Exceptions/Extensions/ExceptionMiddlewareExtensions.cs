using Microsoft.AspNetCore.Builder;
using Qubitlab.CrossCuttingConcerns.Exceptions.Middlewares;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.Extensions;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureCustomExceptionMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionMiddleware>();
}