using Qubitlab.Abstractions.Logging;
using Serilog.Core;
using Serilog.Events;

namespace Qubitlab.Logging.Serilog.Enrichers;

/// <summary>
/// Her log event'e <c>CorrelationId</c> property'si ekler.
/// </summary>
/// <remarks>
/// <see cref="ICorrelationIdProvider"/> üzerinden mevcut HTTP isteğinin
/// X-Correlation-Id değerini okur ve Serilog event'ine atar.
/// Değer yoksa "unknown" yazar.
/// </remarks>
internal sealed class CorrelationIdEnricher : ILogEventEnricher
{
    private readonly ICorrelationIdProvider _provider;

    public CorrelationIdEnricher(ICorrelationIdProvider provider)
        => _provider = provider;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _provider.CorrelationId ?? "unknown";
        var property = propertyFactory.CreateProperty("CorrelationId", correlationId);
        logEvent.AddPropertyIfAbsent(property);
    }
}
