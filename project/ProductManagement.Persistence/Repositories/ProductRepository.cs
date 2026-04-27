using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence.Context;
using Qubitlab.Persistence.EFCore.Repositories;

namespace ProductManagement.Persistence.Repositories;

public class ProductRepository : EfRepositoryBase<Product,Guid,AppDbContext>, IProductRepository
{
    public ProductRepository(AppDbContext context, bool autoSaveChanges = true) : base(context, autoSaveChanges)
    {
    }
}