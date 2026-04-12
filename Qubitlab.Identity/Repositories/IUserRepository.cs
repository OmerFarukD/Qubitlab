using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Repositories;

/// <summary>
/// Generic user repository kontratı.
/// </summary>
/// <typeparam name="TUser">
///     Kullanılacak user tipi. <see cref="QubitlabUser"/> veya ondan türeyen sınıf.
/// </typeparam>
public interface IUserRepository<TUser> where TUser : QubitlabUser
{
    // ─── Sorgular ─────────────────────────────────────────

    /// <summary>ID ile kullanıcı getirir. Bulamazsa <c>null</c> döner.</summary>
    Task<TUser?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>E-posta ile kullanıcı getirir. Bulamazsa <c>null</c> döner.</summary>
    Task<TUser?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcıyı rolleri dahil getirir (eager loading).
    /// Claim üretimi ve yetkilendirme için kullanılır.
    /// </summary>
    Task<TUser?> GetWithRolesAsync(Guid id, CancellationToken ct = default);

    /// <summary>Belirtilen e-posta ile kayıtlı kullanıcı var mı?</summary>
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Sayfalı kullanıcı listesi döner.
    /// </summary>
    /// <param name="page">Sayfa numarası (1 tabanlı)</param>
    /// <param name="pageSize">Sayfa başına kayıt sayısı</param>
    /// <param name="ct">İptal token'ı</param>
    Task<IReadOnlyList<TUser>> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Belirtilen role sahip kullanıcıları getirir.</summary>
    Task<IReadOnlyList<TUser>> GetByRoleAsync(string roleName, CancellationToken ct = default);

    // ─── Komutlar ─────────────────────────────────────────

    /// <summary>Yeni kullanıcı ekler.</summary>
    Task AddAsync(TUser user, CancellationToken ct = default);

    /// <summary>Kullanıcıyı günceller.</summary>
    Task UpdateAsync(TUser user, CancellationToken ct = default);

    /// <summary>
    /// Kullanıcıyı soft delete ile siler.
    /// QubitlabDbContext global query filter sayesinde
    /// sorgulardan otomatik olarak gizlenir.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Varsayılan <see cref="QubitlabUser"/> kullanan repository kontratı.
/// </summary>
public interface IUserRepository : IUserRepository<QubitlabUser> { }
