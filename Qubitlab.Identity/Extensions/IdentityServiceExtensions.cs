using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qubitlab.Identity.Entities;
using Qubitlab.Identity.Repositories;
using Qubitlab.Security.RefreshToken;

namespace Qubitlab.Identity.Extensions;

/// <summary>
/// Qubitlab.Identity paketini DI container'a kayıt eden extension metodlar.
/// </summary>
public static class IdentityServiceExtensions
{
    // ════════════════════════════════════════════════════
    // Overload 1 — Tam varsayılan (en basit kullanım)
    // ════════════════════════════════════════════════════

    /// <summary>
    /// Tüm Qubitlab entity tipleri ile Identity servislerini kayıt eder.
    /// </summary>
    /// <example>
    /// <code>
    /// // AppDbContext'te:
    /// protected override void OnModelCreating(ModelBuilder mb)
    /// {
    ///     base.OnModelCreating(mb);
    ///     mb.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
    /// }
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabIdentity<TContext>(
        this IServiceCollection services)
        where TContext : DbContext
        => services.AddQubitlabIdentity<
            TContext,
            QubitlabUser,
            QubitlabRole,
            QubitlabUserRole,
            IdentityRefreshToken>();

    // ════════════════════════════════════════════════════
    // Overload 2 — Sadece User tipini özelleştir
    // ════════════════════════════════════════════════════

    /// <summary>
    /// Kendi User tipinle Identity servislerini kayıt eder.
    /// Role, UserRole ve RefreshToken varsayılan Qubitlab tipleri kullanılır.
    /// </summary>
    /// <example>
    /// <code>
    /// // Tüketici projede:
    /// public class AppUser : QubitlabUser
    /// {
    ///     public string Department { get; set; } = string.Empty;
    /// }
    ///
    /// builder.Services.AddQubitlabIdentity&lt;AppDbContext, AppUser&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabIdentity<TContext, TUser>(
        this IServiceCollection services)
        where TContext : DbContext
        where TUser : QubitlabUser
        => services.AddQubitlabIdentity<
            TContext,
            TUser,
            QubitlabRole,
            QubitlabUserRole,
            IdentityRefreshToken>();

    // ════════════════════════════════════════════════════
    // Overload 3 — Tam generic (tüm tipleri özelleştir)
    // ════════════════════════════════════════════════════

    /// <summary>
    /// Tüm entity tiplerini özelleştirerek Identity servislerini kayıt eder.
    /// </summary>
    /// <typeparam name="TContext">Tüketici projenin DbContext'i</typeparam>
    /// <typeparam name="TUser">Özel User entity — <see cref="QubitlabUser"/>'dan türemeli</typeparam>
    /// <typeparam name="TRole">Özel Role entity — <see cref="QubitlabRole"/>'dan türemeli</typeparam>
    /// <typeparam name="TUserRole">Özel UserRole entity — <see cref="QubitlabUserRole"/>'dan türemeli</typeparam>
    /// <typeparam name="TToken">Özel RefreshToken entity — <see cref="IdentityRefreshToken"/>'dan türemeli</typeparam>
    /// <example>
    /// <code>
    /// builder.Services.AddQubitlabIdentity&lt;
    ///     AppDbContext,
    ///     AppUser,
    ///     AppRole,
    ///     AppUserRole,
    ///     AppRefreshToken&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabIdentity<TContext, TUser, TRole, TUserRole, TToken>(
        this IServiceCollection services)
        where TContext  : DbContext
        where TUser     : QubitlabUser
        where TRole     : QubitlabRole
        where TUserRole : QubitlabUserRole
        where TToken    : IdentityRefreshToken, new()
    {
        // ── IRefreshTokenRepository ──────────────────────
        services.AddScoped<IRefreshTokenRepository>(sp =>
            new EfRefreshTokenRepository<TToken>(sp.GetRequiredService<TContext>()));

        // ── IUserRepository ───────────────────────────────
        services.AddScoped<IUserRepository<TUser>>(sp =>
            new EfUserRepository<TUser>(sp.GetRequiredService<TContext>()));

        // Varsayılan tipte ise IUserRepository (non-generic) de kaydet
        if (typeof(TUser) == typeof(QubitlabUser))
        {
            services.AddScoped<IUserRepository>(sp =>
                new EfUserRepository(sp.GetRequiredService<TContext>()));
        }

        // ── IRoleRepository ───────────────────────────────
        services.AddScoped<IRoleRepository<TRole>>(sp =>
            new EfRoleRepository<TRole>(sp.GetRequiredService<TContext>()));

        // Varsayılan tipte ise IRoleRepository (non-generic) de kaydet
        if (typeof(TRole) == typeof(QubitlabRole))
        {
            services.AddScoped<IRoleRepository>(sp =>
                new EfRoleRepository(sp.GetRequiredService<TContext>()));
        }

        return services;
    }
}
