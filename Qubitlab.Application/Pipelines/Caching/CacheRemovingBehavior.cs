using MediatR;
using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Application.Pipelines.Caching;

/// <summary>
/// Cache temizleme yapan MediatR pipeline behavior'u.
/// Command başarıyla çalıştıktan sonra ilgili cache key'lerini ve grupları temizler.
/// </summary>
/// <remarks>
/// Akış:
/// <code>
/// İstek → ByPassCache? → Evet: handler'a geç, temizleme yok
///                      → Hayır: handler çalıştır (next)
///                               → CacheGroupKey varsa grubu sil
///                               → CacheKey varsa tek key'i sil
///                               → Response döndür
/// </code>
/// NOT: Temizleme işlemi handler'dan SONRA yapılır.
/// Bu, okuma-yazma tutarsızlığını önler (önce yaz, sonra cache'i geçersiz kıl).
/// </remarks>
/// <typeparam name="TRequest">MediatR request tipi — <see cref="ICacheRemoverRequest"/> implement etmeli</typeparam>
/// <typeparam name="TResponse">Handler'ın döndürdüğü yanıt tipi</typeparam>
public sealed class CacheRemovingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheRemoverRequest
{
    private readonly ICacheService _cache;

    public CacheRemovingBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // ByPassCache → sadece handler çalışır, temizleme yok
        if (request.ByPassCache)
            return await next();

        // Önce handler çalışsın (DB'ye yaz)
        var response = await next();

        // Sonra cache'i temizle — "cache aside" pattern
        if (request.CacheGroupKey is not null)
            await RemoveGroupAsync(request.CacheGroupKey, cancellationToken);

        if (request.CacheKey is not null)
            await _cache.RemoveAsync(request.CacheKey, cancellationToken);

        return response;
    }

    // ────────────────────────────────────────────────────
    // Group temizleme
    // ────────────────────────────────────────────────────

    /// <summary>
    /// Grup key'ine ait tüm cache key'lerini temizler, ardından grup metadatasını siler.
    /// </summary>
    private async Task RemoveGroupAsync(string groupKey, CancellationToken cancellationToken)
    {
        var group = await _cache.GetAsync<CacheGroup>(groupKey, cancellationToken);
        if (group is null) return;

        // Gruptaki her key'i tek tek sil
        foreach (var key in group.Keys)
            await _cache.RemoveAsync(key, cancellationToken);

        // Grup metadatasını da sil
        await _cache.RemoveAsync(groupKey, cancellationToken);
    }
}
