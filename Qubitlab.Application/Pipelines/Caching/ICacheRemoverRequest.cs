namespace Qubitlab.Application.Pipelines.Caching;

/// <summary>
/// Cache temizleme yapılan komutlar için işaretleyici interface.
/// MediatR pipeline'ında <see cref="CacheRemovingBehavior{TRequest,TResponse}"/> tarafından tüketilir.
/// </summary>
/// <example>
/// <code>
/// public class UpdateProductCommand : IRequest&lt;ProductDto&gt;, ICacheRemoverRequest
/// {
///     public int    ProductId       { get; set; }
///     public string Name            { get; set; } = string.Empty;
///
///     // Direkt silinecek tek key (opsiyonel)
///     public string? CacheKey       => $"products:{ProductId}";
///
///     // Grubun tamamı silinecek (opsiyonel)
///     public string? CacheGroupKey  => "products";
///
///     public bool ByPassCache       => false;
/// }
/// </code>
/// </example>
public interface ICacheRemoverRequest
{
    /// <summary>
    /// Kaldırılacak tek ve spesifik cache key.
    /// null bırakılırsa bu adım atlanır.
    /// </summary>
    string? CacheKey { get; }

    /// <summary>
    /// Kaldırılacak cache grubunun key'i.
    /// Bu gruba ait tüm key'ler toplu silinir.
    /// null bırakılırsa grup silme işlemi atlanır.
    /// </summary>
    string? CacheGroupKey { get; }

    /// <summary>
    /// true → cache temizleme atlanır, sadece handler çalışır.
    /// </summary>
    bool ByPassCache { get; }
}
