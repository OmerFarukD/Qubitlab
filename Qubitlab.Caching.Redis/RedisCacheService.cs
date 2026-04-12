using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Caching.Redis;

/// <summary>
/// <see cref="ICacheService"/> implementasyonu — Redis / dağıtık cache.
/// <see cref="IDistributedCache"/> üzerine inşa edilmiştir.
/// Çok sunuculu (multi-node / cloud) senaryolar için uygundur.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly RedisCacheOptions _options;

    // JSON serialize seçenekleri — tüm operasyonlarda paylaşılır
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public RedisCacheService(IDistributedCache cache, IOptions<RedisCacheOptions> options)
    {
        _cache   = cache;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var data = await _cache.GetAsync(BuildKey(key), cancellationToken).ConfigureAwait(false);

        return data is null
            ? default
            : JsonSerializer.Deserialize<T>(data, _jsonOptions);
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

        var data    = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
        var options = BuildDistributedOptions(expiration ?? _options.DefaultExpiration);

        await _cache.SetAsync(BuildKey(key), data, options, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _cache.RemoveAsync(BuildKey(key), cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        var data = await _cache.GetAsync(BuildKey(key), cancellationToken).ConfigureAwait(false);
        return data is not null;
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

        var builtKey = BuildKey(key);

        // Cache hit
        var data = await _cache.GetAsync(builtKey, cancellationToken).ConfigureAwait(false);
        if (data is not null)
            return JsonSerializer.Deserialize<T>(data, _jsonOptions)!;

        // Cache miss — factory çalıştır ve yaz
        var value   = await factory().ConfigureAwait(false);
        var bytes   = JsonSerializer.SerializeToUtf8Bytes(value, _jsonOptions);
        var options = BuildDistributedOptions(expiration ?? _options.DefaultExpiration);

        await _cache.SetAsync(builtKey, bytes, options, cancellationToken).ConfigureAwait(false);

        return value;
    }

    /// <summary>
    /// Tek bir key'i yeniler (expiration süresini resetler).
    /// </summary>
    public async Task RefreshAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        await _cache.RefreshAsync(BuildKey(key), cancellationToken).ConfigureAwait(false);
    }

    // ────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────

    /// <summary>
    /// KeyPrefix varsa key başına ekler.
    /// Örnek: prefix="myapp:" key="users:1" → "myapp:users:1"
    /// </summary>
    private string BuildKey(string key) =>
        string.IsNullOrEmpty(_options.KeyPrefix)
            ? key
            : $"{_options.KeyPrefix}{key}";

    private DistributedCacheEntryOptions BuildDistributedOptions(TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions();

        if (_options.UseSlidingExpiration)
            options.SlidingExpiration = expiration;
        else
            options.AbsoluteExpirationRelativeToNow = expiration;

        return options;
    }
}
