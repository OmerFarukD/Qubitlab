namespace Qubitlab.Persistence.EFCore.Entities;

/// <summary>
/// Serilog EF Core sink tarafından DB'ye yazılan uygulama log kaydı.
/// </summary>
/// <remarks>
/// AuditLog'dan farkı:
/// <list type="bullet">
///   <item><b>AuditLog</b> → Entity değişikliklerini kaydeder (kim ne değiştirdi)</item>
///   <item><b>AppLogEntry</b> → Uygulama loglarını kaydeder (exception, warning, request bilgisi)</item>
/// </list>
/// Id tipi <c>long</c>: yüksek hacimli log tablosunda Guid'den çok daha performanslı.
/// </remarks>
public sealed class AppLogEntry
{
    /// <summary>Otomatik artan birincil anahtar — yüksek hacim için long kullanılır.</summary>
    public long Id { get; set; }

    /// <summary>Log seviyesi: Verbose, Debug, Information, Warning, Error, Fatal.</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>Render edilmiş log mesajı (template + argümanlar birleşik).</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Varsa exception tipi ve stack trace. Yoksa <c>null</c>.</summary>
    public string? Exception { get; set; }

    /// <summary>HTTP isteğini izleyen benzersiz CorrelationId. Serilog enricher'dan gelir.</summary>
    public string? CorrelationId { get; set; }

    /// <summary>Logu tetikleyen kullanıcının ID'si. Anonim isteklerde <c>null</c>.</summary>
    public string? UserId { get; set; }

    /// <summary>Logun üretildiği makine/pod adı. Kubernetes ortamı için önemli.</summary>
    public string? MachineName { get; set; }

    /// <summary>
    /// Log event'e eklenen tüm Serilog property'leri JSON olarak saklanır.
    /// Structured log verisi sorgulanabilir şekilde tutulur.
    /// </summary>
    public string? Properties { get; set; }

    /// <summary>Log event'in tam zaman damgası (UTC).</summary>
    public DateTime Timestamp { get; set; }
}
