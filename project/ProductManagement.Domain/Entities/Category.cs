using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Domain.Entities;

public class Category : Entity<int>
{
    public string Name { get; set; }

    public List<Product> Products { get; set; }
}