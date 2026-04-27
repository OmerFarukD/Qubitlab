namespace ProductManagement.Application.Features.Products.Constants;

public static class ProductMessages
{
    public const string ProductNotFoundMessage = "Product Not Found.";
    public const string ProductNameMustBeUniqueMessage = "Product Name Must Be Unique!";

    // Validation Messages
    public const string NameRequiredMessage = "Product name is required.";
    public const string NameMinLengthMessage = "Product name must be at least 3 characters.";
    public const string NameMaxLengthMessage = "Product name must not exceed 200 characters.";
    public const string PriceRequiredMessage = "Product price is required.";
    public const string PriceMustBePositiveMessage = "Product price must be greater than zero.";
    public const string PriceMaxValueMessage = "Product price cannot exceed 1,000,000.";
    public const string StockCannotBeNegativeMessage = "Product stock cannot be negative.";
    public const string StockMaxValueMessage = "Product stock cannot exceed 1,000,000.";
    public const string DescriptionMaxLengthMessage = "Product description must not exceed 1000 characters.";
    public const string CategoryIdRequiredMessage = "Category ID is required.";
    public const string IdRequiredMessage = "Product ID is required.";
}
