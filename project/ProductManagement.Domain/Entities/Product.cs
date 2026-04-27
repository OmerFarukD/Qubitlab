using Qubitlab.Persistence.EFCore.Entities;

namespace ProductManagement.Domain.Entities;

public class Product : Entity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; }
    
}