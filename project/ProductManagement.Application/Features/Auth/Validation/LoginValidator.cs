using FluentValidation;
using ProductManagement.Application.Features.Auth.Constants;

namespace ProductManagement.Application.Features.Auth.Validation;

public sealed class LoginValidator : AbstractValidator<Commands.Login.LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage(AuthMessages.EmailRequiredMessage)
            .EmailAddress().WithMessage(AuthMessages.EmailInvalidMessage);

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage(AuthMessages.PasswordRequiredMessage);
    }
}
