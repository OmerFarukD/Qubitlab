using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Qubitlab.Logging.Serilog.Enrichers;

internal sealed class CorrelationIdEnricher : ILogEventEnricher
{
    private const string CorrelationIdKey = "CorrelationId";
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelationIdEnricher(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = _httpContextAccessor.HttpContext?.Items[CorrelationIdKey]?.ToString()
                            ?? "unknown";

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty(CorrelationIdKey, correlationId));
    }
}
