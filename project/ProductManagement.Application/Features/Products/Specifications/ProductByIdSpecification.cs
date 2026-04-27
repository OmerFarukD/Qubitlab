using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Specifications;

namespace ProductManagement.Application.Features.Products.Specifications;

public sealed class ProductByIdSpecification : BaseSpecification<Product>
{
    public ProductByIdSpecification(Guid id)
        : base(p => p.Id == id)
    {
    }
}
