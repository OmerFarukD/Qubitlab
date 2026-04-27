namespace ProductManagement.Application.Features.Categories.Constants;

public static class CategoryMessages
{
    public const string CategoryNotFoundMessage = "Category Not Found.";
    public const string CategoryNameMustBeUniqueMessage = "Category Name Must Be Unique!";

    // Validation Messages
    public const string NameRequiredMessage = "Category name is required.";
    public const string NameMinLengthMessage = "Category name must be at least 3 characters.";
    public const string NameMaxLengthMessage = "Category name must not exceed 200 characters.";
    public const string IdRequiredMessage = "Category ID is required.";
}
