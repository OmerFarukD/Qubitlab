namespace Qubitlab.Caching.Hybrid;

/// <summary>
/// Hybrid cache konfigürasyon seçenekleri.
/// L1 (in-process) ve L2 (Redis) katmanlarını ayrı ayrı yapılandırmanıza olanak tanır.
/// </summary>
public sealed class HybridCacheOptions
{
    // ─── L2 Redis ───────────────────────────────────────

    /// <summary>
    /// Redis bağlantı string'i (L2 katmanı).
    /// null veya boş bırakılırsa yalnızca L1 (in-process) cache çalışır.
    /// Örnek: "localhost:6379" | "redis:6379,password=secret,ssl=true"
    /// </summary>
    public string? RedisConnectionString { get; set; }

    /// <summary>
    /// Redis InstanceName — key'lere prefix olarak eklenir.
    /// </summary>
    public string RedisInstanceName { get; set; } = string.Empty;

    // ─── Expiration ──────────────────────────────────────

    /// <summary>
    /// L1 (in-process) cache için varsayılan yaşam süresi.
    /// Varsayılan: 5 dakika — L1 her zaman L2'den kısa olmalı.
    /// </summary>
    public TimeSpan DefaultLocalExpiration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// L2 (Redis) cache için varsayılan yaşam süresi.
    /// Varsayılan: 30 dakika.
    /// </summary>
    public TimeSpan DefaultDistributedExpiration { get; set; } = TimeSpan.FromMinutes(30);
}
