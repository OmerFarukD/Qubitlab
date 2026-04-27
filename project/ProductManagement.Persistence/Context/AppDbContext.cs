using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using Qubitlab.Abstractions.Security;
using Qubitlab.Identity.Configurations;
using Qubitlab.Identity.Entities;
using Qubitlab.Persistence.EFCore.Context;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Logging;

namespace ProductManagement.Persistence.Context;

public sealed class AppDbContext : QubitlabDbContext<AppDbContext>, IHasAppLogs
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUserService = null)
        : base(options, currentUserService)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<QubitlabUser> Users => Set<QubitlabUser>();
    public DbSet<QubitlabRole> Roles => Set<QubitlabRole>();
    public DbSet<QubitlabUserRole> UserRoles => Set<QubitlabUserRole>();
    public DbSet<IdentityRefreshToken> RefreshTokens => Set<IdentityRefreshToken>();
    public DbSet<AppLogEntry> AppLogs => Set<AppLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ConfigureAppLogs();

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.City).HasMaxLength(100);
            b.Property(u => u.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(UserConfiguration).Assembly);
    }
}
