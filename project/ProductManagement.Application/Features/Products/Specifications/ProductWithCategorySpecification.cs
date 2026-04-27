using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductWithCategorySpecification : BaseSpecification<Product>
{
    public ProductWithCategorySpecification()
    {
        AddInclude(p => p.Category);
    }
}
