using ProductManagement.Application.Services.Repositories;
using ProductManagement.Domain.Entities;
using ProductManagement.Persistence.Context;
using Qubitlab.Persistence.EFCore.Repositories;

namespace ProductManagement.Persistence.Repositories;

public class CategoryRepository : EfRepositoryBase<Category, int, AppDbContext>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context, bool autoSaveChanges = true) : base(context, autoSaveChanges)
    {
    }
}