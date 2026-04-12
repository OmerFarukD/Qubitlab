using Microsoft.Extensions.DependencyInjection;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.Redis;

/// <summary>
/// Redis cache servisini DI container'a kayıt eden extension metodlar.
/// </summary>
public static class RedisCacheServiceExtensions
{
    /// <summary>
    /// <see cref="ICacheService"/> implementasyonu olarak <see cref="RedisCacheService"/> kaydeder.
    /// </summary>
    /// <param name="services">DI konteyneri</param>
    /// <param name="configure">Redis konfigürasyon seçenekleri</param>
    /// <example>
    /// <code>
    /// // Basit kullanım
    /// builder.Services.AddQubitlabRedisCache(opt =>
    ///     opt.ConnectionString = "localhost:6379");
    ///
    /// // Tam konfigürasyon
    /// builder.Services.AddQubitlabRedisCache(opt =>
    /// {
    ///     opt.ConnectionString      = "redis-server:6379,password=secret,ssl=true";
    ///     opt.KeyPrefix             = "myapp:";
    ///     opt.DefaultExpiration     = TimeSpan.FromHours(1);
    ///     opt.UseSlidingExpiration  = false;
    ///     opt.InstanceName          = "MyAppCache";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabRedisCache(
        this IServiceCollection services,
        Action<RedisCacheOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        // Options'ı kaydet
        services.Configure(configure);

        // StackExchangeRedis'i kaydet
        // Options'dan ConnectionString ve InstanceName alınır
        services.AddStackExchangeRedisCache(redisOpt =>
        {
            // Geçici bir instance oluşturarak Options değerlerine eriş
            var opt = new RedisCacheOptions();
            configure(opt);

            redisOpt.Configuration  = opt.ConnectionString;
            redisOpt.InstanceName   = opt.InstanceName;
        });
        
        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }
}
