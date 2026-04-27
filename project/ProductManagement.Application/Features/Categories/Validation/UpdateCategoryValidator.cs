using FluentValidation;
using ProductManagement.Application.Features.Categories.Constants;

namespace ProductManagement.Application.Features.Categories.Validation;

public sealed class UpdateCategoryValidator : AbstractValidator<Commands.Update.CategoryUpdateCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage(CategoryMessages.IdRequiredMessage)
            .GreaterThan(0).WithMessage(CategoryMessages.IdRequiredMessage);

        RuleFor(c => c.Name)
            .NotEmpty().WithMessage(CategoryMessages.NameRequiredMessage)
            .MinimumLength(3).WithMessage(CategoryMessages.NameMinLengthMessage)
            .MaximumLength(200).WithMessage(CategoryMessages.NameMaxLengthMessage);
    }
}
