using FluentValidation;
using ProductManagement.Application.Features.Products.Constants;

namespace ProductManagement.Application.Features.Products.Validation;

public sealed class UpdateProductValidator : AbstractValidator<Commands.Update.ProductUpdateCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage(ProductMessages.IdRequiredMessage);

        RuleFor(p => p.Name)
            .NotEmpty().WithMessage(ProductMessages.NameRequiredMessage)
            .MinimumLength(3).WithMessage(ProductMessages.NameMinLengthMessage)
            .MaximumLength(200).WithMessage(ProductMessages.NameMaxLengthMessage);

        RuleFor(p => p.Price)
            .NotEmpty().WithMessage(ProductMessages.PriceRequiredMessage)
            .GreaterThan(0).WithMessage(ProductMessages.PriceMustBePositiveMessage)
            .LessThanOrEqualTo(1_000_000).WithMessage(ProductMessages.PriceMaxValueMessage);

        RuleFor(p => p.Stock)
            .GreaterThanOrEqualTo(0).WithMessage(ProductMessages.StockCannotBeNegativeMessage)
            .LessThanOrEqualTo(1_000_000).WithMessage(ProductMessages.StockMaxValueMessage);

        RuleFor(p => p.Description)
            .MaximumLength(1000).WithMessage(ProductMessages.DescriptionMaxLengthMessage)
            .When(p => p.Description is not null);

        RuleFor(p => p.CategoryId)
            .NotEmpty().WithMessage(ProductMessages.CategoryIdRequiredMessage)
            .GreaterThan(0).WithMessage(ProductMessages.CategoryIdRequiredMessage);
    }
}
