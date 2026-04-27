namespace ProductManagement.Application.Features.Products.Commands.Create;

public sealed class ProductAddedResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int CategoryId { get; set; }
}
