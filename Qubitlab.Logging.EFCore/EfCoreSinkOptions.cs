using Serilog.Events;

namespace Qubitlab.Logging.EFCore;

/// <summary>
/// <see cref="EfCoreLogSink{TContext}"/> için yapılandırma seçenekleri.
/// </summary>
public sealed class EfCoreSinkOptions
{
    // ── Batch yazma ayarları ─────────────────────────────────────

    /// <summary>
    /// Tek seferde DB'ye yazılacak maksimum log sayısı.
    /// Varsayılan: <c>50</c>.
    /// </summary>
    /// <remarks>
    /// Çok düşük → sık DB çağrısı (yük artar).
    /// Çok yüksek → memory baskısı ve batch gecikmesi.
    /// 50-100 arası production için uygundur.
    /// </remarks>
    public int BatchSizeLimit { get; set; } = 50;

    /// <summary>
    /// Batch'in DB'ye yazılma periyodu. Dolmamış batch bu süre sonunda yine yazılır.
    /// Varsayılan: <c>5 saniye</c>.
    /// </summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Bellek içi log kuyruğunun maksimum kapasitesi.
    /// Kuyruk dolunca yeni loglar DROP edilir (ana uygulama etkilenmez).
    /// Varsayılan: <c>10.000</c>.
    /// </summary>
    public int QueueLimit { get; set; } = 10_000;

    // ── Hata yönetimi ────────────────────────────────────────────

    /// <summary>
    /// DB yazma başarısız olduğunda kaç kez yeniden denenir.
    /// Varsayılan: <c>3</c>.
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Retry denemeleri arasındaki bekleme süresi.
    /// Varsayılan: <c>2 saniye</c>.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(2);

    // ── Log filtreleme ───────────────────────────────────────────

    /// <summary>
    /// DB'ye yazılacak minimum log seviyesi.
    /// Konsol/dosyaya göre daha yüksek set edilmesi önerilir.
    /// Varsayılan: <c>Warning</c> (sadece Warning, Error ve Critical DB'ye gider).
    /// </summary>
    /// <remarks>
    /// Tüm logları DB'ye yazmak büyük yük oluşturur. Production'da
    /// minimum <c>Warning</c> veya <c>Error</c> önerilir.
    /// </remarks>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Warning;

    /// <summary>
    /// <c>true</c> ise Serilog property'lerinin tamamı JSON olarak
    /// <see cref="Qubitlab.Persistence.EFCore.Entities.AppLogEntry.Properties"/> alanına yazılır.
    /// <c>false</c> ise sadece temel alanlar (CorrelationId, UserId, MachineName) kaydedilir.
    /// Varsayılan: <c>true</c>.
    /// </summary>
    public bool CaptureProperties { get; set; } = true;
}
