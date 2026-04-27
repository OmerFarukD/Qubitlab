using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductByCategorySpecification : BaseSpecification<Product>
{
    public ProductByCategorySpecification(int categoryId)
        : base(p => p.CategoryId == categoryId)
    {
    }
}
