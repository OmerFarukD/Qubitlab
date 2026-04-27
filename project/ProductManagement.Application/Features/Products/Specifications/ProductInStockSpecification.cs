using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductInStockSpecification : BaseSpecification<Product>
{
    public ProductInStockSpecification()
        : base(p => p.Stock > 0)
    {
    }
}
