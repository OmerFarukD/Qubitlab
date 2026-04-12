using Microsoft.Extensions.Caching.Memory;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.InMemory;

/// <summary>
/// <see cref="ICacheService"/> implementasyonu.
/// Microsoft.Extensions.Caching.Memory üzerine inşa edilmiştir.
/// Tek sunucu / tek proses senaryoları için uygundur.
/// Dağıtık (multi-node) sistemlerde Redis kullanın.
/// </summary>
public sealed class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _defaultExpiration;

    // Registered key'leri takip etmek için (ExistsAsync + RemoveByPrefix desteği)
    private readonly HashSet<string> _keys = [];
    private readonly Lock _keyLock = new();

    public InMemoryCacheService(IMemoryCache cache, TimeSpan? defaultExpiration = null)
    {
        _cache = cache;
        _defaultExpiration = defaultExpiration ?? TimeSpan.FromMinutes(30);
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var value = _cache.TryGetValue(key, out T? cached) ? cached : default;
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(value);

        var options = BuildOptions(expiration ?? _defaultExpiration, key);
        _cache.Set(key, value, options);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        _cache.Remove(key);

        lock (_keyLock)
            _keys.Remove(key);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(factory);

        if (_cache.TryGetValue(key, out T? cached) && cached is not null)
            return cached;

        // Cache miss — factory'yi çağır
        var value = await factory().ConfigureAwait(false);

        var options = BuildOptions(expiration ?? _defaultExpiration, key);
        _cache.Set(key, value, options);

        return value;
    }

    /// <summary>
    /// Belirli bir prefix ile başlayan tüm key'leri cache'den temizler.
    /// </summary>
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);

        List<string> toRemove;
        lock (_keyLock)
            toRemove = [.. _keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal))];

        foreach (var key in toRemove)
        {
            _cache.Remove(key);
            lock (_keyLock)
                _keys.Remove(key);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Tüm cache'i temizler.
    /// NOT: IMemoryCache.Clear() sadece .NET 9+ IMemoryCache2'de mevcut.
    /// Bu metod takip edilen key'leri tek tek siler.
    /// </summary>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        List<string> snapshot;
        lock (_keyLock)
            snapshot = [.. _keys];

        foreach (var key in snapshot)
            _cache.Remove(key);

        lock (_keyLock)
            _keys.Clear();

        return Task.CompletedTask;
    }

    // ────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────

    private MemoryCacheEntryOptions BuildOptions(TimeSpan expiration, string key)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration,
            // Expiry olduğunda key setinden otomatik sil
            PostEvictionCallbacks =
            {
                new PostEvictionCallbackRegistration
                {
                    EvictionCallback = (evictedKey, _, _, _) =>
                    {
                        lock (_keyLock)
                            _keys.Remove(evictedKey.ToString()!);
                    }
                }
            }
        };

        lock (_keyLock)
            _keys.Add(key);

        return options;
    }
}
