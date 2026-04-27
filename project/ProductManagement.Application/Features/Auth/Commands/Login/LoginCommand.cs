using MediatR;
using ProductManagement.Application.Features.Auth.Constants;
using ProductManagement.Application.Features.Auth.Rules;
using ProductManagement.Application.Services.Auth;
using ProductManagement.Domain.Entities;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Qubitlab.Security.Hashing;

namespace ProductManagement.Application.Features.Auth.Commands.Login;

public sealed class LoginCommand : IRequest<LoggedInResponseDto>
{
    public string Email { get; set; }
    public string Password { get; set; }

    public sealed class LoginCommandHandler(
        IAuthService _authService,
        IHashingService _hashingService,
        AuthBusinessRules _businessRules
    ) : IRequestHandler<LoginCommand, LoggedInResponseDto>
    {
        public async Task<LoggedInResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            User? user = await _authService.FindByEmailAsync(request.Email, cancellationToken);

            if (user is null)
                throw new BusinessException(AuthMessages.InvalidCredentialsMessage);

            AuthBusinessRules.UserMustBeActive(user);

            if (!_hashingService.Verify(request.Password, user.PasswordHash))
                throw new BusinessException(AuthMessages.InvalidCredentialsMessage);

            var accessToken = await _authService.CreateAccessTokenAsync(user, cancellationToken);
            var refreshToken = await _authService.CreateRefreshTokenAsync(
                user.Id,
                accessToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value,
                cancellationToken);

            return new LoggedInResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken.Token,
                AccessTokenExpiresAt = accessToken.ExpiresAt
            };
        }
    }
}