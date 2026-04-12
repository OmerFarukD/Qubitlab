using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Repositories;

/// <summary>
/// Generic role repository kontratı.
/// </summary>
/// <typeparam name="TRole">
///     Kullanılacak role tipi. <see cref="QubitlabRole"/> veya ondan türeyen sınıf.
/// </typeparam>
public interface IRoleRepository<TRole> where TRole : QubitlabRole
{
    // ─── Sorgular ─────────────────────────────────────────

    /// <summary>ID ile rol getirir. Bulamazsa <c>null</c> döner.</summary>
    Task<TRole?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Rol adı ile getirir (case-insensitive). Bulamazsa <c>null</c> döner.</summary>
    Task<TRole?> GetByNameAsync(string name, CancellationToken ct = default);

    /// <summary>Tüm aktif rolleri getirir.</summary>
    Task<IReadOnlyList<TRole>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Belirtilen adda rol var mı? (case-insensitive)</summary>
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

    // ─── Komutlar ─────────────────────────────────────────

    /// <summary>Yeni rol ekler.</summary>
    Task AddAsync(TRole role, CancellationToken ct = default);

    /// <summary>Rolü günceller.</summary>
    Task UpdateAsync(TRole role, CancellationToken ct = default);

    /// <summary>Rolü soft delete ile siler.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Varsayılan <see cref="QubitlabRole"/> kullanan repository kontratı.
/// </summary>
public interface IRoleRepository : IRoleRepository<QubitlabRole> { }
