using MediatR;
using Microsoft.Extensions.Options;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Application.Pipelines.Caching;

/// <summary>
/// Cache'den okuma yapan MediatR pipeline behavior'u.
/// <see cref="ICachableRequest"/> implementasyonlarını otomatik cache'ler.
/// </summary>
/// <remarks>
/// Akış:
/// <code>
/// İstek → ByPassCache? → Evet: handler'a geç
///                      → Hayır: cache'de var mı?
///                               → Evet: cache'den döndür (DB'ye gitme)
///                               → Hayır: handler çalıştır → cache'e yaz → döndür
/// </code>
/// </remarks>
/// <typeparam name="TRequest">MediatR request tipi — <see cref="ICachableRequest"/> implement etmeli</typeparam>
/// <typeparam name="TResponse">Handler'ın döndürdüğü yanıt tipi</typeparam>
public sealed class CachingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly ICacheService _cache;
    private readonly CachePipelineSettings _settings;

    public CachingBehavior(ICacheService cache, IOptions<CachePipelineSettings> settings)
    {
        _cache    = cache;
        _settings = settings.Value;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // ByPassCache → direk handler'a git, cache yok say
        if (request.ByPassCache)
            return await next();

        // Cache'de var mı? → varsa döndür, yoksa factory (next) çalıştır ve yaz
        var sliding = request.SlidingExpiration ?? _settings.DefaultSlidingExpiration;

        var response = await _cache.GetOrCreateAsync(
            key:        request.CacheKey,
            factory:    async () => await next(),
            expiration: sliding,
            cancellationToken: cancellationToken
        );

        // Grup key'i varsa, bu key'i gruba kaydet
        if (request.CacheGroupKey is not null)
            await AddKeyToGroupAsync(request.CacheKey, request.CacheGroupKey, sliding, cancellationToken);

        return response;
    }

    // ────────────────────────────────────────────────────
    // Group yönetimi
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Verilen cache key'ini gruba ekler.
    /// Grup metadatası CacheGroup record'u olarak cache'de tutulur.
    /// </summary>
    private async Task AddKeyToGroupAsync(
        string cacheKey,
        string groupKey,
        TimeSpan slidingExpiration,
        CancellationToken cancellationToken)
    {
        var group = await _cache.GetAsync<CacheGroup>(groupKey, cancellationToken)
                    ?? CacheGroup.Empty();

        group.Keys.Add(cacheKey);

        // Gruptaki en uzun expiration'ı ticks olarak takip et
        var newTicks = slidingExpiration.Ticks > group.MaxSlidingExpirationTicks
            ? slidingExpiration.Ticks
            : group.MaxSlidingExpirationTicks;

        group = group with { MaxSlidingExpirationTicks = newTicks };

        var groupExpiration = TimeSpan.FromTicks(newTicks) * _settings.GroupKeyExpirationMultiplier;

        await _cache.SetAsync(groupKey, group, groupExpiration, cancellationToken);
    }
}

/// <summary>
/// Cache grubunun metadata şeması.
/// Hangi key'lerin bu gruba ait olduğunu ve grubun max expiration süresini tutar.
/// </summary>
/// <remarks>
/// <b>Neden Ticks (long)?</b><br/>
/// InMemory → obje olarak tutulur, herhangi bir tip çalışır.<br/>
/// Redis / Hybrid L2 → JSON serialize edilir. <c>TimeSpan</c> JSON'da
/// <c>"00:30:00"</c> formatına dönüşür ancak <c>System.Text.Json</c>
/// varsayılan ayarlarında geri deserialize edilemez.<br/>
/// <c>long</c> (Ticks) her backend'de güvenle serialize/deserialize edilir.
/// </remarks>
/// <param name="Keys">Bu gruba kayıtlı cache key'leri</param>
/// <param name="MaxSlidingExpirationTicks">
///     Gruptaki key'lerin maksimum sliding expiration süresi (TimeSpan.Ticks cinsinden).
/// </param>
internal sealed record CacheGroup(HashSet<string> Keys, long MaxSlidingExpirationTicks)
{
    /// <summary>MaxSlidingExpirationTicks değerini TimeSpan olarak döndürür.</summary>
    public TimeSpan MaxSlidingExpiration => TimeSpan.FromTicks(MaxSlidingExpirationTicks);

    /// <summary>TimeSpan'den CacheGroup oluşturmak için factory.</summary>
    public static CacheGroup Empty() => new([], 0L);
}
