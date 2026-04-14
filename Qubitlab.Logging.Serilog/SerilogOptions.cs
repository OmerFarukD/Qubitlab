using Microsoft.Extensions.Logging;

namespace Qubitlab.Logging.Serilog;

/// <summary>
/// <c>AddQubitlabSerilog()</c> extension metodu için yapılandırma modeli.
/// appsettings.json içinde <c>Qubitlab:Logging</c> bölümüne bağlanır.
/// </summary>
public sealed class SerilogOptions
{
    // ── Sink ayarları ────────────────────────────────────────────

    /// <summary>Konsole log yazılsın mı? Varsayılan: <c>true</c>.</summary>
    public bool WriteToConsole { get; set; } = true;

    /// <summary>
    /// Konsol çıktısı için format.
    /// <list type="bullet">
    ///   <item><c>"Text"</c> — insan okunabilir (geliştirme)</item>
    ///   <item><c>"CompactJson"</c> — CLEF JSON (production, Seq, Loki)</item>
    /// </list>
    /// Varsayılan: <c>"Text"</c>.
    /// </summary>
    public string ConsoleFormat { get; set; } = "Text";

    /// <summary>Dosyaya log yazılsın mı? Varsayılan: <c>false</c>.</summary>
    public bool WriteToFile { get; set; } = false;

    /// <summary>Log dosyası yolu. Varsayılan: <c>logs/log-.json</c>.</summary>
    public string FilePath { get; set; } = "logs/log-.json";

    /// <summary>Dosya rotasyon periyodu. Varsayılan: <c>"Day"</c>.</summary>
    public string RollingInterval { get; set; } = "Day";

    /// <summary>Saklanan log dosyası sayısı. Varsayılan: 30.</summary>
    public int RetainedFileCount { get; set; } = 30;

    /// <summary>Seq'e log yazılsın mı? Varsayılan: <c>false</c>.</summary>
    public bool WriteToSeq { get; set; } = false;

    /// <summary>Seq sunucusunun URL adresi. Örn: <c>http://localhost:5341</c>.</summary>
    public string? SeqUrl { get; set; }

    /// <summary>Seq API anahtarı (isteğe bağlı).</summary>
    public string? SeqApiKey { get; set; }

    // ── Enrichment ayarları ─────────────────────────────────────

    /// <summary>CorrelationId her log'a eklensin mi? Varsayılan: <c>true</c>.</summary>
    public bool EnrichWithCorrelationId { get; set; } = true;

    /// <summary>Kullanıcı bilgileri (UserId, Email) her log'a eklensin mi? Varsayılan: <c>true</c>.</summary>
    public bool EnrichWithCurrentUser { get; set; } = true;

    /// <summary>Makine adı her log'a eklensin mi? Varsayılan: <c>true</c>.</summary>
    public bool EnrichWithMachineName { get; set; } = true;

    /// <summary>Thread ID her log'a eklensin mi? Varsayılan: <c>false</c>.</summary>
    public bool EnrichWithThreadId { get; set; } = false;

    // ── Level ayarları ──────────────────────────────────────────

    /// <summary>Varsayılan minimum log seviyesi. Varsayılan: <c>Information</c>.</summary>
    public LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    /// <summary>
    /// Microsoft namespace için override log seviyesi.
    /// Varsayılan: <c>Warning</c> (System spamini engeller).
    /// </summary>
    public LogLevel MicrosoftMinimumLevel { get; set; } = LogLevel.Warning;

    // ── Performance behavior ayarları ──────────────────────────

    /// <summary>
    /// Bu süreyi aşan MediatR request'leri PerformanceBehavior tarafından
    /// Warning olarak loglanır. Varsayılan: <c>500</c> ms.
    /// </summary>
    public int SlowRequestThresholdMs { get; set; } = 500;
}
