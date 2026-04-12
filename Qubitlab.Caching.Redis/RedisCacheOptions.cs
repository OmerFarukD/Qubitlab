namespace Qubitlab.Caching.Redis;

/// <summary>
/// Redis cache servisinin konfigürasyon seçenekleri.
/// </summary>
public sealed class RedisCacheOptions
{
    /// <summary>
    /// Redis bağlantı string'i.
    /// Örnek: "localhost:6379" veya "redis-server:6379,password=secret,ssl=true"
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";

    /// <summary>
    /// Tüm key'lere eklenecek prefix. Farklı uygulamaları aynı Redis sunucusunda izole eder.
    /// Örnek: "myapp:" → key "users:1" → Redis'te "myapp:users:1"
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Varsayılan cache süresi. SetAsync'de expiration belirtilmezse bu değer kullanılır.
    /// Varsayılan: 30 dakika.
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Serialization için kullanılacak instance name.
    /// Microsoft.Extensions.Caching.StackExchangeRedis'in InstanceName özelliğiyle eşleşir.
    /// </summary>
    public string InstanceName { get; set; } = string.Empty;

    /// <summary>
    /// Sliding expiration kullanılsın mı?
    /// true → her erişimde süre sıfırlanır.
    /// false → absolute expiration uygulanır (varsayılan).
    /// </summary>
    public bool UseSlidingExpiration { get; set; } = false;
}
