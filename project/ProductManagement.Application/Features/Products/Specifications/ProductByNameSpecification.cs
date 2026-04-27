using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductByNameSpecification : BaseSpecification<Product>
{
    public ProductByNameSpecification(string name)
        : base(p => p.Name.Contains(name))
    {
    }
}
