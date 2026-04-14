using Microsoft.Extensions.Logging;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.Logging.Serilog;

/// <summary>
/// <see cref="IAppLogger{T}"/>'nin Serilog tabanlı implementasyonu.
/// </summary>
/// <remarks>
/// Doğrudan <see cref="ILogger{T}"/> üzerine oturur; bu sayede:
/// <list type="bullet">
///   <item>MEL pipeline'ı tam çalışır (filter, scope, provider zinciri)</item>
///   <item>Serilog enricher'lar (CorrelationId, UserId...) otomatik devreye girer</item>
///   <item>Her yerde <c>IAppLogger&lt;T&gt;</c> inject edilebilir, vendor bağımlılığı sıfır</item>
/// </list>
/// </remarks>
internal sealed class AppLogger<T> : IAppLogger<T>
{
    private readonly ILogger<T> _inner;

    public AppLogger(ILogger<T> inner) => _inner = inner;

    // ── Temel metodlar ──────────────────────────────────────────

    public void LogTrace(string messageTemplate, params object?[] args)
        => _inner.LogTrace(messageTemplate, args);

    public void LogDebug(string messageTemplate, params object?[] args)
        => _inner.LogDebug(messageTemplate, args);

    public void LogInformation(string messageTemplate, params object?[] args)
        => _inner.LogInformation(messageTemplate, args);

    public void LogWarning(string messageTemplate, params object?[] args)
        => _inner.LogWarning(messageTemplate, args);

    public void LogError(Exception? exception, string messageTemplate, params object?[] args)
        => _inner.LogError(exception, messageTemplate, args);

    public void LogCritical(Exception? exception, string messageTemplate, params object?[] args)
        => _inner.LogCritical(exception, messageTemplate, args);

    // ── EventId overload'ları ────────────────────────────────────

    public void LogInformation(EventId eventId, string messageTemplate, params object?[] args)
        => _inner.LogInformation(eventId, messageTemplate, args);

    public void LogWarning(EventId eventId, string messageTemplate, params object?[] args)
        => _inner.LogWarning(eventId, messageTemplate, args);

    public void LogError(EventId eventId, Exception? exception, string messageTemplate, params object?[] args)
        => _inner.LogError(eventId, exception, messageTemplate, args);

    // ── Yardımcı metodlar ───────────────────────────────────────

    public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

    public IDisposable BeginScope<TState>(TState state) where TState : notnull
        => _inner.BeginScope(state) ?? NullScope.Instance;

    // ── Null scope guard ────────────────────────────────────────

    /// <summary>
    /// ILogger.BeginScope() bazı provider'larda null dönebilir.
    /// Null-guard olarak kullanılır; Dispose çağrısı güvenlidir.
    /// </summary>
    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        private NullScope() { }
        public void Dispose() { }
    }
}
