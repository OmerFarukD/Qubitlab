namespace ProductManagement.Application.Features.Products.Commands.Delete;

public sealed class ProductDeletedResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
