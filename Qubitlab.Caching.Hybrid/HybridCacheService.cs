using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Options;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.Hybrid;

/// <summary>
/// <see cref="ICacheService"/> implementasyonu — HybridCache (L1 + L2).
/// <br/>
/// <b>L1:</b> Process içi IMemoryCache — nanosecond erişim süresi.<br/>
/// <b>L2:</b> Redis IDistributedCache — milisecond erişim süresi, node'lar arası paylaşım.<br/>
/// <br/>
/// <b>Stampede koruması:</b> Aynı key için eş zamanlı 1000 istek cache miss yaşasa bile
/// factory yalnızca <i>bir kez</i> çağrılır; diğerleri sonucu bekler.
/// </summary>
public sealed class HybridCacheService : ICacheService
{
    private readonly HybridCache _hybridCache;
    private readonly HybridCacheOptions _options;

    public HybridCacheService(HybridCache hybridCache, IOptions<HybridCacheOptions> options)
    {
        _hybridCache = hybridCache;
        _options     = options.Value;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Önce L1 (in-process), sonra L2 (Redis) kontrol edilir.
    /// Her iki katmanda da yoksa <c>default</c> döner — factory çağrılmaz.
    /// </remarks>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        // HybridCache'in TryGetValue API'si yoktur — yalnızca GetOrCreateAsync vardır.
        // Cache miss durumunda factory sonucunu cache'e YAZMAMAK için
        // HybridCacheEntryFlags.DisableLocalCacheWrite | DisableDistributedCacheWrite
        // kullanılır. Böylece null değer kalıcı olarak cache'lenmez.
        var result = await _hybridCache.GetOrCreateAsync<T?>(
            key,
            _ => ValueTask.FromResult(default(T?)),
            new HybridCacheEntryOptions
            {
                Expiration              = _options.DefaultDistributedExpiration,
                LocalCacheExpiration    = _options.DefaultLocalExpiration,
                Flags                   = HybridCacheEntryFlags.DisableLocalCacheWrite
                                          | HybridCacheEntryFlags.DisableDistributedCacheWrite
            },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var distributed = expiration ?? _options.DefaultDistributedExpiration;
        // L1 her zaman L2'den kısa veya eşit olmalı
        var local = distributed < _options.DefaultLocalExpiration
            ? distributed
            : _options.DefaultLocalExpiration;

        // SetAsync — factory yerine direkt değer yaz
        await _hybridCache.SetAsync(
            key,
            value,
            new HybridCacheEntryOptions
            {
                Expiration           = distributed,
                LocalCacheExpiration = local
            },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _hybridCache.RemoveAsync(key, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    /// <remarks>
    /// HybridCache doğrudan "key var mı?" sorgusu desteklemez.
    /// GetAsync çağrısı yapılır; null dönerse yoktur.
    /// </remarks>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = await GetAsync<object>(key, cancellationToken).ConfigureAwait(false);
        return value is not null;
    }

    /// <inheritdoc />
    /// <remarks>
    /// HybridCache'in doğal <c>GetOrCreateAsync</c> metodunu kullanır.
    /// Stampede koruması (cache lock) dahilidir — factory tek seferlik çalışır.
    /// </remarks>
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        var distributed = expiration ?? _options.DefaultDistributedExpiration;
        var local = distributed < _options.DefaultLocalExpiration
            ? distributed
            : _options.DefaultLocalExpiration;

        return await _hybridCache.GetOrCreateAsync(
            key,
            _ => new ValueTask<T>(factory()),   // Func<Task<T>> → ValueTask<T> dönüşüm
            new HybridCacheEntryOptions
            {
                Expiration           = distributed,
                LocalCacheExpiration = local
            },
            cancellationToken: cancellationToken
        ).ConfigureAwait(false);
    }

    /// <summary>
    /// Tag ile işaretlenmiş tüm key'leri geçersiz kılar.
    /// Örnek: "users" tag'i → tüm kullanıcı cache'lerini temizler.
    /// </summary>
    /// <param name="tags">Geçersiz kılınacak tag listesi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    public async Task InvalidateByTagsAsync(
        IEnumerable<string> tags,
        CancellationToken cancellationToken = default)
    {
        await _hybridCache.RemoveByTagAsync(tags, cancellationToken).ConfigureAwait(false);
    }
}
