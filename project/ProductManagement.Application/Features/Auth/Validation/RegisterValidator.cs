using FluentValidation;
using ProductManagement.Application.Features.Auth.Constants;

namespace ProductManagement.Application.Features.Auth.Validation;

public sealed class RegisterValidator : AbstractValidator<Commands.Register.RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(r => r.FullName)
            .NotEmpty().WithMessage(AuthMessages.FullNameRequiredMessage)
            .MinimumLength(2).WithMessage(AuthMessages.FullNameMinLengthMessage)
            .MaximumLength(150).WithMessage(AuthMessages.FullNameMaxLengthMessage);

        RuleFor(r => r.Email)
            .NotEmpty().WithMessage(AuthMessages.EmailRequiredMessage)
            .EmailAddress().WithMessage(AuthMessages.EmailInvalidMessage)
            .MaximumLength(256).WithMessage(AuthMessages.EmailMaxLengthMessage);

        RuleFor(r => r.Password)
            .NotEmpty().WithMessage(AuthMessages.PasswordRequiredMessage)
            .MinimumLength(8).WithMessage(AuthMessages.PasswordMinLengthMessage)
            .MaximumLength(128).WithMessage(AuthMessages.PasswordMaxLengthMessage)
            .Matches(@"[A-Z]").WithMessage(AuthMessages.PasswordComplexityMessage)
            .Matches(@"[a-z]").WithMessage(AuthMessages.PasswordComplexityMessage)
            .Matches(@"[0-9]").WithMessage(AuthMessages.PasswordComplexityMessage)
            .Matches(@"[^a-zA-Z0-9]").WithMessage(AuthMessages.PasswordComplexityMessage);

        RuleFor(r => r.City)
            .NotEmpty().WithMessage(AuthMessages.CityRequiredMessage)
            .MaximumLength(100).WithMessage(AuthMessages.CityMaxLengthMessage);
    }
}
