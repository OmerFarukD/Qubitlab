using Qubitlab.Persistence.EFCore.Specifications;

namespace Qubitlab.Persistence.EFCore.Extensions;

/// <summary>
/// Specification'ları birleştirmek için fluent (zincirleme) extension metodlar.
/// </summary>
/// <example>
/// <code>
/// var spec = new ActiveProductSpec()
///     .And(new InStockSpec())
///     .Or(new FeaturedProductSpec())
///     .Not();
/// </code>
/// </example>
public static class SpecificationExtensions
{
    /// <summary>
    /// İki specification'ı AND (&&) ile birleştirir.
    /// </summary>
    public static ISpecification<T> And<T>(this ISpecification<T> left, ISpecification<T> right)
        => new AndSpecification<T>(left, right);

    /// <summary>
    /// İki specification'ı OR (||) ile birleştirir.
    /// </summary>
    public static ISpecification<T> Or<T>(this ISpecification<T> left, ISpecification<T> right)
        => new OrSpecification<T>(left, right);

    /// <summary>
    /// Specification'ı tersine çevirir (NOT).
    /// </summary>
    public static ISpecification<T> Not<T>(this ISpecification<T> specification)
        => new NotSpecification<T>(specification);
}