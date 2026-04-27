using System.Security.Claims;
using ProductManagement.Domain.Entities;
using Qubitlab.Security.Jwt;
using Qubitlab.Security.RefreshToken;

namespace ProductManagement.Application.Services.Auth;

public interface IAuthService
{
    Task<AccessToken> CreateAccessTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, string? accessTokenJti = null, CancellationToken cancellationToken = default);
    Task<List<Claim>> BuildUserClaimsAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task CreateUserAsync(User user, CancellationToken cancellationToken = default);
}
