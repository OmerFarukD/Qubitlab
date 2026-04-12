using Microsoft.Extensions.DependencyInjection;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.Hybrid;

/// <summary>
/// HybridCache servisini DI container'a kayıt eden extension metodlar.
/// </summary>
public static class HybridCacheServiceExtensions
{
    /// <summary>
    /// <see cref="ICacheService"/> implementasyonu olarak <see cref="HybridCacheService"/> kaydeder.
    /// L1 (in-process) + L2 (Redis) çift katmanlı cache yapılandırır.
    /// </summary>
    /// <param name="services">DI konteyneri</param>
    /// <param name="configure">Hybrid cache konfigürasyon seçenekleri</param>
    /// <example>
    /// <code>
    /// // Sadece L1 (Redis bağlantısı olmadan)
    /// builder.Services.AddQubitlabHybridCache(opt =>
    /// {
    ///     opt.DefaultLocalExpiration       = TimeSpan.FromMinutes(5);
    ///     opt.DefaultDistributedExpiration = TimeSpan.FromMinutes(30);
    /// });
    ///
    /// // L1 + L2 Redis
    /// builder.Services.AddQubitlabHybridCache(opt =>
    /// {
    ///     opt.RedisConnectionString        = "localhost:6379";
    ///     opt.RedisInstanceName            = "MyApp";
    ///     opt.DefaultLocalExpiration       = TimeSpan.FromMinutes(5);
    ///     opt.DefaultDistributedExpiration = TimeSpan.FromHours(2);
    /// });
    ///
    /// // Tag bazlı invalidation (GetOrCreate ile)
    /// var product = await cache.GetOrCreateAsync(
    ///     key:     "products:1",
    ///     factory: () => repo.GetByIdAsync(1));
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabHybridCache(
        this IServiceCollection services,
        Action<HybridCacheOptions>? configure = null)
    {
        var options = new HybridCacheOptions();
        configure?.Invoke(options);

        // Options'ı DI'ye kaydet
        services.Configure<HybridCacheOptions>(opt =>
        {
            opt.RedisConnectionString        = options.RedisConnectionString;
            opt.RedisInstanceName            = options.RedisInstanceName;
            opt.DefaultLocalExpiration       = options.DefaultLocalExpiration;
            opt.DefaultDistributedExpiration = options.DefaultDistributedExpiration;
        });

        // L2 — Redis backend (opsiyonel)
        if (!string.IsNullOrWhiteSpace(options.RedisConnectionString))
        {
            services.AddStackExchangeRedisCache(redisOpt =>
            {
                redisOpt.Configuration = options.RedisConnectionString;
                redisOpt.InstanceName  = options.RedisInstanceName;
            });
        }

        // .NET 9+ HybridCache — L1+L2 otomatik yönetim + stampede koruması
        services.AddHybridCache(hybridOpt =>
        {
            hybridOpt.DefaultEntryOptions = new()
            {
                Expiration           = options.DefaultDistributedExpiration,
                LocalCacheExpiration = options.DefaultLocalExpiration
            };
        });

        // ICacheService → HybridCacheService
        services.AddSingleton<ICacheService, HybridCacheService>();

        return services;
    }
}
