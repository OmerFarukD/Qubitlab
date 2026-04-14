using Microsoft.AspNetCore.Http;
using Qubitlab.Abstractions.Logging;
using Qubitlab.CrossCuttingConcerns.Exceptions.Handlers;

namespace Qubitlab.CrossCuttingConcerns.Exceptions.Middlewares;

/// <summary>
/// Uygulama genelinde yakalanmayan exception'ları merkezi olarak işler.
/// </summary>
/// <remarks>
/// Davranış:
/// <list type="bullet">
///   <item>Exception'ı <see cref="IAppLogger{T}"/> ile CorrelationId ve exception tipiyle loglar</item>
///   <item><see cref="HttpExceptionHandler"/> aracılığıyla RFC 7807 ProblemDetails formatına dönüştürür</item>
///   <item>Pipeline sırası: <c>CorrelationIdMiddleware</c>'den sonra olmalıdır</item>
/// </list>
/// </remarks>
public sealed class ExceptionMiddleware(
    RequestDelegate next,
    IAppLogger<ExceptionMiddleware> logger,
    ICorrelationIdProvider correlationIdProvider)
{
    private readonly HttpExceptionHandler _exceptionHandler = new();

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (Exception ex)
        {
            var correlationId = correlationIdProvider.CorrelationId;
            var exceptionType = ex.GetType().Name;

            logger.LogError(
                ex,
                "✗ [{CorrelationId}] İşlenmeyen hata: {ExceptionType} — {Message}",
                correlationId, exceptionType, ex.Message);

            await HandleExceptionAsync(httpContext.Response, ex);
        }
    }

    private Task HandleExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = "application/problem+json";
        _exceptionHandler.Response = response;
        return _exceptionHandler.HandleExceptionAsync(exception);
    }
}