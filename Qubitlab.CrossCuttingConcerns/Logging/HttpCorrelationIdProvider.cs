using Microsoft.AspNetCore.Http;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.CrossCuttingConcerns.Logging;

public sealed class HttpCorrelationIdProvider : ICorrelationIdProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCorrelationIdProvider(IHttpContextAccessor httpContextAccessor)
        => _httpContextAccessor = httpContextAccessor;

    public string CorrelationId =>
        _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString()
        ?? "unknown";

    public void Set(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        if (_httpContextAccessor.HttpContext is not null)
            _httpContextAccessor.HttpContext.Items["CorrelationId"] = correlationId;
    }
}
