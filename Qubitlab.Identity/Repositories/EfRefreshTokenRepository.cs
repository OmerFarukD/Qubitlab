using Microsoft.EntityFrameworkCore;
using Qubitlab.Identity.Entities;
using Qubitlab.Security.RefreshToken;

namespace Qubitlab.Identity.Repositories;

/// <summary>
/// <see cref="IRefreshTokenRepository"/> generic EF Core implementasyonu.
/// </summary>
/// <typeparam name="TToken">
///     Kullanılacak refresh token entity tipi.
///     <see cref="IdentityRefreshToken"/> veya ondan türeyen kendi sınıfın.
/// </typeparam>
public class EfRefreshTokenRepository<TToken> : IRefreshTokenRepository
    where TToken : IdentityRefreshToken, new()
{
    private readonly DbContext _context;
    private readonly DbSet<TToken> _tokens;

    public EfRefreshTokenRepository(DbContext context)
    {
        _context = context;
        _tokens  = context.Set<TToken>();
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
    {
        var entity = await _tokens
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Token == token, ct);

        return entity?.ToPoco();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<RefreshToken>> GetActiveByUserIdAsync(
        string userId,
        CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var guidId))
            return [];

        var entities = await _tokens
            .AsNoTracking()
            .Where(t => t.UserId == guidId
                     && !t.IsRevoked
                     && !t.IsUsed
                     && t.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(ct);

        return entities.Select(e => e.ToPoco()).ToList().AsReadOnly();
    }

    /// <inheritdoc />
    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
    {
        if (!Guid.TryParse(token.UserId, out var guidId))
            throw new ArgumentException("UserId geçerli bir Guid değil.", nameof(token));

        // IdentityRefreshToken.FromPoco() → TToken'a cast
        var entity = (TToken)IdentityRefreshToken.FromPoco(token, guidId);
        await _tokens.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string token, CancellationToken ct = default)
    {
        var entity = await _tokens.FirstOrDefaultAsync(t => t.Token == token, ct);
        if (entity is null) return;

        entity.IsRevoked = true;
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task RevokeAllByUserIdAsync(string userId, CancellationToken ct = default)
    {
        if (!Guid.TryParse(userId, out var guidId)) return;

        await _tokens
            .Where(t => t.UserId == guidId && !t.IsRevoked)
            .ExecuteUpdateAsync(
                s => s.SetProperty(t => t.IsRevoked, true), ct);
    }

    /// <inheritdoc />
    public async Task DeleteExpiredAsync(CancellationToken ct = default)
    {
        await _tokens
            .Where(t => t.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(ct);
    }
}

/// <summary>
/// Varsayılan <see cref="IdentityRefreshToken"/> kullanan repository.
/// </summary>
public sealed class EfRefreshTokenRepository : EfRefreshTokenRepository<IdentityRefreshToken>
{
    public EfRefreshTokenRepository(DbContext context) : base(context) { }
}
