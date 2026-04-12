using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.InMemory;

/// <summary>
/// In-memory cache servisini DI container'a kayıt eden extension metodlar.
/// </summary>
public static class InMemoryCacheServiceExtensions
{
    /// <summary>
    /// <see cref="ICacheService"/> implementasyonu olarak <see cref="InMemoryCacheService"/> kaydeder.
    /// </summary>
    /// <param name="services">DI konteyneri</param>
    /// <param name="defaultExpiration">
    ///     Varsayılan cache süresi. Belirtilmezse 30 dakika uygulanır.
    /// </param>
    /// <param name="configure">
    ///     İsteğe bağlı <see cref="MemoryCacheOptions"/> konfigürasyonu.
    /// </param>
    /// <example>
    /// <code>
    /// // Basit kullanım
    /// builder.Services.AddQubitlabInMemoryCache();
    ///
    /// // Özelleştirilmiş kullanım
    /// builder.Services.AddQubitlabInMemoryCache(
    ///     defaultExpiration: TimeSpan.FromMinutes(15),
    ///     configure: opt =>
    ///     {
    ///         opt.SizeLimit = 1024;
    ///         opt.CompactionPercentage = 0.25;
    ///     });
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabInMemoryCache(
        this IServiceCollection services,
        TimeSpan? defaultExpiration = null,
        Action<MemoryCacheOptions>? configure = null)
    {
        if (configure is not null)
            services.AddMemoryCache(configure);
        else
            services.AddMemoryCache();

        services.AddSingleton<ICacheService>(sp =>
        {
            var cache = sp.GetRequiredService<IMemoryCache>();
            return new InMemoryCacheService(cache, defaultExpiration);
        });

        return services;
    }
}
