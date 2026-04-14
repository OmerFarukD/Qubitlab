namespace Qubitlab.Abstractions.Logging;

/// <summary>
/// Tüm loglara otomatik eklenecek context (zenginleştirme) bilgileri.
/// Serilog Enricher'ları bu interface'i kullanarak her log event'e
/// CorrelationId, UserId ve TenantId gibi alanları ekler.
/// </summary>
/// <remarks>
/// DI'ya Scoped olarak kayıt edilmeli; böylece her HTTP isteği
/// kendi bağımsız context'ine sahip olur.
/// </remarks>
public interface ILogContext
{
    /// <summary>
    /// İstek boyunca tüm loglara eklenen benzersiz izleme kimliği.
    /// X-Correlation-Id HTTP header'ından veya otomatik üretilir.
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    /// Kimliği doğrulanmış kullanıcının ID'si. Oturum açılmamışsa <c>null</c>.
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Multi-tenant yapılarda mevcut tenant'ın kimliği. Tekil sistemlerde <c>null</c>.
    /// </summary>
    string? TenantId { get; }

    /// <summary>
    /// OpenTelemetry / W3C Trace Context standardındaki trace kimliği.
    /// Distributed tracing ile loglama arasında köprü kurar.
    /// </summary>
    string? TraceId { get; }
}
