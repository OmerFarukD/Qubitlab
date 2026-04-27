using ProductManagement.Domain.Entities;
using Qubitlab.Persistence.EFCore.Repositories;

namespace ProductManagement.Application.Services.Repositories;

public interface ICategoryRepository : IAsyncRepository<Category, int>
{
    
}