namespace Qubitlab.Application.Pipelines.Caching;

/// <summary>
/// Cache pipeline davranışlarının konfigürasyon ayarları.
/// appsettings.json'dan <c>CachePipeline</c> section'ı ile bind edilir.
/// </summary>
/// <example>
/// appsettings.json:
/// <code>
/// {
///   "CachePipeline": {
///     "DefaultSlidingExpirationMinutes": 30,
///     "GroupKeyExpirationMultiplier": 2.0
///   }
/// }
/// </code>
/// </example>
public sealed class CachePipelineSettings
{
    /// <summary>appsettings.json section adı.</summary>
    public const string SectionName = "CachePipeline";

    /// <summary>
    /// ICachableRequest.SlidingExpiration null olduğunda kullanılacak varsayılan süre (dakika).
    /// Varsayılan: 30 dakika.
    /// </summary>
    public int DefaultSlidingExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// Grup key'inin expiration süresi = gruptaki en uzun key süresi × bu çarpan.
    /// Grubun kendi key'lerinden önce expire olmamasını garantiler.
    /// Varsayılan: 1.5 (1.5x)
    /// </summary>
    public double GroupKeyExpirationMultiplier { get; set; } = 1.5;

    /// <summary>Hesaplanmış varsayılan sliding expiration (TimeSpan).</summary>
    public TimeSpan DefaultSlidingExpiration =>
        TimeSpan.FromMinutes(DefaultSlidingExpirationMinutes);
}
