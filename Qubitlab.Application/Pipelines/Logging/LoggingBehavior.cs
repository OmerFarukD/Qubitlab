using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.Application.Pipelines.Logging;

/// <summary>
/// Her MediatR request/response çiftini structured olarak loglar.
/// </summary>
/// <remarks>
/// Logladıkları:
/// <list type="bullet">
///   <item>→ Request başlangıcı: istek adı, CorrelationId, payload</item>
///   <item>← Request tamamlanması: istek adı, geçen süre (ms)</item>
///   <item>✗ Hata durumunda: exception ile birlikte hata logu</item>
/// </list>
///
/// Hassas veri koruması:
/// Request tipi <see cref="ISensitiveRequest"/> implement ediyorsa
/// payload loglanmaz; bunun yerine <c>[REDACTED]</c> yazılır.
///
/// <b>Pipeline sırası:</b> İlk behavior olmalıdır.
/// Örnek kayıt:
/// <code>
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(LoggingBehavior&lt;,&gt;));
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(PerformanceBehavior&lt;,&gt;));
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(ValidationBehavior&lt;,&gt;));
/// services.AddTransient(typeof(IPipelineBehavior&lt;,&gt;), typeof(CachingBehavior&lt;,&gt;));
/// </code>
/// </remarks>
/// <typeparam name="TRequest">MediatR request tipi.</typeparam>
/// <typeparam name="TResponse">Handler'ın döndürdüğü yanıt tipi.</typeparam>
public sealed class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAppLogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICorrelationIdProvider _correlationId;

    public LoggingBehavior(
        IAppLogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICorrelationIdProvider correlationId)
    {
        _logger        = logger;
        _correlationId = correlationId;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName    = typeof(TRequest).Name;
        var correlationId  = _correlationId.CorrelationId;
        var isSensitive    = request is ISensitiveRequest;

        // ── Request başlangıç logu ───────────────────────────────
        if (_logger.IsEnabled(LogLevel.Information))
        {
            if (isSensitive)
            {
                _logger.LogInformation(
                    "→ [{CorrelationId}] {RequestName} başladı. [Hassas payload gizlendi]",
                    correlationId, requestName);
            }
            else
            {
                _logger.LogInformation(
                    "→ [{CorrelationId}] {RequestName} başladı. Payload: {@Request}",
                    correlationId, requestName, request);
            }
        }

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next();
            sw.Stop();

            // ── Başarılı tamamlanma logu ─────────────────────────
            _logger.LogInformation(
                "← [{CorrelationId}] {RequestName} tamamlandı. Süre: {ElapsedMs}ms",
                correlationId, requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            // ── Hata logu ────────────────────────────────────────
            _logger.LogError(
                ex,
                "✗ [{CorrelationId}] {RequestName} hata verdi. Süre: {ElapsedMs}ms",
                correlationId, requestName, sw.ElapsedMilliseconds);

            throw; // exception'ı yukarı taşı — ExceptionMiddleware yakalar
        }
    }
}

/// <summary>
/// Bu interface'i implement eden MediatR request'lerin payload'ı
/// <see cref="LoggingBehavior{TRequest,TResponse}"/> tarafından loglanmaz.
/// </summary>
/// <example>
/// <code>
/// public sealed record LoginCommand(string Email, string Password)
///     : IRequest&lt;LoginResponse&gt;, ISensitiveRequest;
/// </code>
/// </example>
public interface ISensitiveRequest { }
