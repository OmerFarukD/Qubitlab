using FluentValidation;
using ProductManagement.Application.Features.Categories.Constants;

namespace ProductManagement.Application.Features.Categories.Validation;

public sealed class CreateCategoryValidator : AbstractValidator<Commands.Create.CategoryAddCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(CategoryMessages.NameRequiredMessage)
            .MinimumLength(3).WithMessage(CategoryMessages.NameMinLengthMessage)
            .MaximumLength(200).WithMessage(CategoryMessages.NameMaxLengthMessage);
    }
}
