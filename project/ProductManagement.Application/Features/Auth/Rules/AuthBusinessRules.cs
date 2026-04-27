using ProductManagement.Application.Features.Auth.Constants;
using ProductManagement.Application.Services.Auth;
using Qubitlab.Application.BaseBusiness;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;

namespace ProductManagement.Application.Features.Auth.Rules;

public class AuthBusinessRules : BaseBusinessRules
{
    private readonly IAuthService _authService;

    public AuthBusinessRules(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task EmailMustBeUniqueAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _authService.FindByEmailAsync(email, cancellationToken);
        if (user is not null)
            throw new BusinessException(AuthMessages.UserAlreadyExistsMessage);
    }

    public static void UserMustBeActive(Domain.Entities.User user)
    {
        if (!user.IsActive)
            throw new BusinessException(AuthMessages.AccountIsDeactivatedMessage);
    }
}
