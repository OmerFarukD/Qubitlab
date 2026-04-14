using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.Application.Pipelines.Logging;

/// <summary>
/// Yavaş çalışan MediatR request'leri tespit edip Warning olarak loglar.
/// </summary>
/// <remarks>
/// <see cref="PerformancePipelineSettings.SlowRequestThresholdMs"/> değerini aşan
/// her request için şu bilgilerle Warning logu yazılır:
/// <list type="bullet">
///   <item>Request adı</item>
///   <item>Geçen süre (ms)</item>
///   <item>Eşik süresi (ms)</item>
///   <item>Request payload'u (<see cref="ISensitiveRequest"/> değilse)</item>
/// </list>
///
/// <b>Pipeline sırası:</b> <see cref="LoggingBehavior{TRequest,TResponse}"/>'dan
/// sonra, <c>ValidationBehavior</c>'dan önce olmalıdır.
/// </remarks>
/// <typeparam name="TRequest">MediatR request tipi.</typeparam>
/// <typeparam name="TResponse">Handler'ın döndürdüğü yanıt tipi.</typeparam>
public sealed class PerformanceBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAppLogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly PerformancePipelineSettings _settings;

    public PerformanceBehavior(
        IAppLogger<PerformanceBehavior<TRequest, TResponse>> logger,
        IOptions<PerformancePipelineSettings> settings)
    {
        _logger   = logger;
        _settings = settings.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var sw       = Stopwatch.StartNew();
        var response = await next();
        sw.Stop();

        var elapsed   = sw.ElapsedMilliseconds;
        var threshold = _settings.SlowRequestThresholdMs;

        if (elapsed > threshold)
        {
            var requestName = typeof(TRequest).Name;
            var isSensitive = request is ISensitiveRequest;

            if (isSensitive)
            {
                _logger.LogWarning(
                    "⚠️ YAVAŞ REQUEST: {RequestName} {ElapsedMs}ms sürdü. (Eşik: {ThresholdMs}ms) [Hassas payload gizlendi]",
                    requestName, elapsed, threshold);
            }
            else
            {
                _logger.LogWarning(
                    "⚠️ YAVAŞ REQUEST: {RequestName} {ElapsedMs}ms sürdü. (Eşik: {ThresholdMs}ms) Payload: {@Request}",
                    requestName, elapsed, threshold, request);
            }
        }

        return response;
    }
}
