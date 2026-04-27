using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using Qubitlab.Identity.Entities;
using Qubitlab.Identity.Repositories;
using Qubitlab.Security.Jwt;
using Qubitlab.Security.RefreshToken;

namespace ProductManagement.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserRepository<User> _userRepository;

    public AuthService(
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        IUserRepository<User> userRepository)
    {
        _jwtService = jwtService;
        _refreshTokenService = refreshTokenService;
        _userRepository = userRepository;
    }

    public async Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        List<Claim> claims = await BuildUserClaimsAsync(user, cancellationToken);
        return _jwtService.CreateToken(claims);
    }

    public async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? accessTokenJti = null, CancellationToken cancellationToken = default)
    {
        return await _refreshTokenService.CreateAsync(userId.ToString(), accessTokenJti, cancellationToken);
    }

    public async Task<List<Claim>> BuildUserClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName)
        };

        User? userWithRoles = await _userRepository.GetWithRolesAsync(user.Id, cancellationToken);
        if (userWithRoles?.UserRoles is not null)
        {
            foreach (var userRole in userWithRoles.UserRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.Name));
            }
        }

        return claims;
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetByEmailAsync(email, cancellationToken);
    }

    public async Task CreateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _userRepository.AddAsync(user, cancellationToken);
    }
}
