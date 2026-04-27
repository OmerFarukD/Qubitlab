using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;
using Qubitlab.Identity.Configurations;

namespace ProductManagement.Persistence.Configurations;

/// <summary>
/// <see cref="User"/> entity'sinin EF Core Fluent API konfigürasyonu.
/// <para>
/// Qubitlab'ın <see cref="UserConfiguration{TUser}"/> generic base konfigürasyonundan türer.
/// Bu sayede <c>Users</c> tablosundaki tüm temel alanlar (Email, PasswordHash, vb.)
/// otomatik olarak yapılandırılır. Sadece <see cref="User"/>'a özgü alanlar burada eklenir.
/// </para>
/// </summary>
public sealed class AppUserConfiguration : UserConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        // Qubitlab base konfigürasyonu önce uygula (Users tablosu, Email index'i vb.)
        base.Configure(builder);

        // ─── User'a özgü ek alanlar ───────────────────────────
        builder.Property(u => u.City)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(u => u.ImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}
