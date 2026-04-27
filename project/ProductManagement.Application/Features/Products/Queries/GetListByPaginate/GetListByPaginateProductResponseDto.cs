namespace ProductManagement.Application.Features.Products.Queries.GetListByPaginate;

public sealed class GetListByPaginateProductResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}
