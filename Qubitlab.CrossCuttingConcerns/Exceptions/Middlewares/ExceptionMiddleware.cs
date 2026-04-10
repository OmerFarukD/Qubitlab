using Microsoft.AspNetCore.Http;
using Qubitlab.CrossCuttingConcerns.Exceptions.Handlers;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.Middlewares;

public class ExceptionMiddleware(RequestDelegate next)
{
    private readonly HttpExceptionHandler _exceptionHandler = new();


    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception e)
        {
         
            await HandleExceptionAsync(httpContext.Response, e);
        }

    }
    
    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/problem+json";
        _exceptionHandler.Response = response;
        return _exceptionHandler.HandleExceptionAsync(exception);
    }
}