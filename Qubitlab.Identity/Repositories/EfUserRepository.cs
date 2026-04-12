using Microsoft.EntityFrameworkCore;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Repositories;

/// <summary>
/// <see cref="IUserRepository{TUser}"/> generic EF Core implementasyonu.
/// </summary>
public class EfUserRepository<TUser> : IUserRepository<TUser>
    where TUser : QubitlabUser
{
    private readonly DbContext _context;
    private readonly DbSet<TUser> _users;

    public EfUserRepository(DbContext context)
    {
        _context = context;
        _users   = context.Set<TUser>();
    }

    /// <inheritdoc />
    public async Task<TUser?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <inheritdoc />
    public async Task<TUser?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToUpperInvariant();
        return await _users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.NormalizedEmail == normalized, ct);
    }

    /// <inheritdoc />
    public async Task<TUser?> GetWithRolesAsync(Guid id, CancellationToken ct = default)
        => await _users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <inheritdoc />
    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.ToUpperInvariant();
        return await _users.AnyAsync(u => u.NormalizedEmail == normalized, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TUser>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return await _users
            .AsNoTracking()
            .OrderBy(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TUser>> GetByRoleAsync(
        string roleName,
        CancellationToken ct = default)
    {
        var normalized = roleName.ToUpperInvariant();
        return await _users
            .AsNoTracking()
            .Where(u => u.UserRoles
                .Any(ur => ur.Role.NormalizedName == normalized))
            .ToListAsync(ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(TUser user, CancellationToken ct = default)
    {
        // NormalizedEmail otomatik doldur
        if (string.IsNullOrEmpty(user.NormalizedEmail))
            user.NormalizedEmail = user.Email.ToUpperInvariant();

        await _users.AddAsync(user, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TUser user, CancellationToken ct = default)
    {
        // NormalizedEmail senkron tut
        user.NormalizedEmail = user.Email.ToUpperInvariant();

        _users.Update(user);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _users.FirstOrDefaultAsync(u => u.Id == id, ct);
        if (user is null) return;

        // Soft delete — QubitlabDbContext global filter sayesinde artık sorgulanmaz
        user.IsDeleted   = true;
        user.DeletedTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Varsayılan <see cref="QubitlabUser"/> kullanan user repository.
/// </summary>
public sealed class EfUserRepository : EfUserRepository<QubitlabUser>, IUserRepository
{
    public EfUserRepository(DbContext context) : base(context) { }
}
