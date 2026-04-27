using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductByPriceRangeSpecification : BaseSpecification<Product>
{
    public ProductByPriceRangeSpecification(decimal? minPrice = null, decimal? maxPrice = null)
    {
        if (minPrice.HasValue && maxPrice.HasValue)
            Criteria = p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value;
        else if (minPrice.HasValue)
            Criteria = p => p.Price >= minPrice.Value;
        else if (maxPrice.HasValue)
            Criteria = p => p.Price <= maxPrice.Value;
    }
}
