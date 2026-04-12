using Microsoft.EntityFrameworkCore;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Repositories;

/// <summary>
/// <see cref="IRoleRepository{TRole}"/> generic EF Core implementasyonu.
/// </summary>
public class EfRoleRepository<TRole> : IRoleRepository<TRole>
    where TRole : QubitlabRole
{
    private readonly DbContext _context;
    private readonly DbSet<TRole> _roles;

    public EfRoleRepository(DbContext context)
    {
        _context = context;
        _roles   = context.Set<TRole>();
    }

    /// <inheritdoc />
    public async Task<TRole?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    /// <inheritdoc />
    public async Task<TRole?> GetByNameAsync(string name, CancellationToken ct = default)
    {
        var normalized = name.ToUpperInvariant();
        return await _roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalized, ct);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TRole>> GetAllAsync(CancellationToken ct = default)
        => await _roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
    {
        var normalized = name.ToUpperInvariant();
        return await _roles.AnyAsync(r => r.NormalizedName == normalized, ct);
    }

    /// <inheritdoc />
    public async Task AddAsync(TRole role, CancellationToken ct = default)
    {
        // NormalizedName otomatik doldur
        if (string.IsNullOrEmpty(role.NormalizedName))
            role.NormalizedName = role.Name.ToUpperInvariant();

        await _roles.AddAsync(role, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(TRole role, CancellationToken ct = default)
    {
        // NormalizedName senkron tut
        role.NormalizedName = role.Name.ToUpperInvariant();

        _roles.Update(role);
        await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var role = await _roles.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (role is null) return;

        role.IsDeleted   = true;
        role.DeletedTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Varsayılan <see cref="QubitlabRole"/> kullanan role repository.
/// </summary>
public sealed class EfRoleRepository : EfRoleRepository<QubitlabRole>, IRoleRepository
{
    public EfRoleRepository(DbContext context) : base(context) { }
}
