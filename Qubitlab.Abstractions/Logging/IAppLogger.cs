using Microsoft.Extensions.Logging;

namespace Qubitlab.Abstractions.Logging;

/// <summary>
/// Qubitlab ekosistemi için structured logging kontratı.
/// Arka planda Serilog, NLog veya herhangi bir MEL-uyumlu provider kullanılabilir.
/// </summary>
/// <typeparam name="T">Log kategorisi olarak kullanılacak sınıf.</typeparam>
public interface IAppLogger<T>
{
    // ─────────────────────────────────────────────────────────────
    // Temel log metodları — Serilog message template sözdizimi:
    //   "Kullanıcı {UserId} {@User} ile giriş yaptı"
    //   {@} → objeyi destructure eder (JSON olarak loglanır)
    //   {$} → ToString() ile string'e dönüştürür
    // ─────────────────────────────────────────────────────────────

    /// <summary>Çok ayrıntılı geliştirici logları. Production'da genellikle kapalı tutulur.</summary>
    void LogTrace(string messageTemplate, params object?[] args);

    /// <summary>Geliştirme aşamasında hata ayıklama logları.</summary>
    void LogDebug(string messageTemplate, params object?[] args);

    /// <summary>Uygulamanın normal akışını takip eden bilgi logları.</summary>
    void LogInformation(string messageTemplate, params object?[] args);

    /// <summary>Beklenmedik ama kurtarılabilir durumlar için uyarı logları.</summary>
    void LogWarning(string messageTemplate, params object?[] args);

    /// <summary>İşlemi durduran ama uygulamayı çökertmeyen hata logları.</summary>
    void LogError(Exception? exception, string messageTemplate, params object?[] args);

    /// <summary>Uygulamanın durmasını gerektiren kritik hata logları.</summary>
    void LogCritical(Exception? exception, string messageTemplate, params object?[] args);

    // ─────────────────────────────────────────────────────────────
    // EventId overload'ları — log filtering ve monitoring için
    // Örnek: new EventId(1001, "UserLoggedIn")
    // ─────────────────────────────────────────────────────────────

    /// <inheritdoc cref="LogInformation(string, object?[])"/>
    void LogInformation(EventId eventId, string messageTemplate, params object?[] args);

    /// <inheritdoc cref="LogWarning(string, object?[])"/>
    void LogWarning(EventId eventId, string messageTemplate, params object?[] args);

    /// <inheritdoc cref="LogError(Exception?, string, object?[])"/>
    void LogError(EventId eventId, Exception? exception, string messageTemplate, params object?[] args);

    // ─────────────────────────────────────────────────────────────
    // Yardımcı metodlar
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Belirtilen log seviyesi aktif mi kontrol eder.
    /// Pahalı nesne oluşturmadan önce kontrol edin:
    /// <code>if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug(...);</code>
    /// </summary>
    bool IsEnabled(LogLevel logLevel);

    /// <summary>
    /// Scoped log context açar. using bloğu boyunca tüm loglara
    /// belirtilen state eklenir. Disposal ile scope kapanır.
    /// <code>using (_logger.BeginScope(new { OrderId = 42 })) { ... }</code>
    /// </summary>
    IDisposable BeginScope<TState>(TState state) where TState : notnull;
}
