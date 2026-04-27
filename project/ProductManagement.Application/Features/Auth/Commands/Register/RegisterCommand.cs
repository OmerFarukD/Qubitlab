using System.Security.Claims;
using MediatR;
using ProductManagement.Application.Features.Auth.Constants;
using ProductManagement.Application.Features.Auth.Rules;
using ProductManagement.Application.Services.Auth;
using ProductManagement.Domain.Entities;
using Qubitlab.CrossCuttingConcerns.Exceptions.ExceptionTypes;
using Qubitlab.Security.Hashing;

namespace ProductManagement.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommand : IRequest<RegisteredResponseDto>
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string City { get; set; }

    public sealed class RegisterCommandHandler(
        IAuthService _authService,
        IHashingService _hashingService,
        AuthBusinessRules _businessRules
    ) : IRequestHandler<RegisterCommand, RegisteredResponseDto>
    {
        public async Task<RegisteredResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            await _businessRules.EmailMustBeUniqueAsync(request.Email, cancellationToken);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpperInvariant(),
                PasswordHash = _hashingService.Hash(request.Password),
                City = request.City,
                IsActive = true
            };

            await _authService.CreateUserAsync(user, cancellationToken);

            var accessToken = await _authService.CreateAccessTokenAsync(user, cancellationToken);
            var refreshToken = await _authService.CreateRefreshTokenAsync(user.Id, accessToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value, cancellationToken);

            return new RegisteredResponseDto
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
