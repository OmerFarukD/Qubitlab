namespace Qubitlab.Application.Pipelines.Caching;

/// <summary>
/// Cache'den okuma yapılan request'ler için işaretleyici interface.
/// MediatR pipeline'ında <see cref="CachingBehavior{TRequest,TResponse}"/> tarafından tüketilir.
/// </summary>
/// <example>
/// <code>
/// public class GetProductsQuery : IRequest&lt;List&lt;ProductDto&gt;&gt;, ICachableRequest
/// {
///     // Cache key — istek parametrelerine göre benzersiz olmalı
///     public string CacheKey        => "products:list";
///
///     // true → cache atlanır, her seferinde DB'ye gidilir  
///     public bool   ByPassCache     => false;
///
///     // Grup key → "products" grubundaki tüm key'leri silmek için
///     public string? CacheGroupKey  => "products";
///
///     // null → CachePipelineSettings.DefaultSlidingExpiration kullanılır  
///     public TimeSpan? SlidingExpiration => null;
/// }
/// </code>
/// </example>
public interface ICachableRequest
{
    /// <summary>Cache key — istek parametrelerine göre benzersiz olmalı.</summary>
    string CacheKey { get; }

    /// <summary>
    /// true → cache atlanır, handler her çağrıda DB'ye gider.
    /// Geçici debug / admin senaryoları için kullanılır.
    /// </summary>
    bool ByPassCache { get; }

    /// <summary>
    /// Grup key — birden fazla cache key'ini mantıksal olarak gruplar.
    /// Örn: "products" → GetProductsList ve GetProductById her ikisi de bu gruba eklenirse,
    /// bir ürün güncellendiğinde "products" grubunu silmek yeterli olur.
    /// </summary>
    string? CacheGroupKey { get; }

    /// <summary>
    /// Bu istek için özel sliding expiration süresi.
    /// null → <see cref="CachePipelineSettings.DefaultSlidingExpiration"/> kullanılır.
    /// </summary>
    TimeSpan? SlidingExpiration { get; }
}
