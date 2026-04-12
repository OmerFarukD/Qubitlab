using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Configurations;

/// <summary>
/// Generic User entity konfigürasyonu.
/// <typeparam name="TUser">
///   Konfigüre edilecek kullanıcı tipi. <see cref="QubitlabUser"/> veya ondan türeyen bir sınıf.
/// </typeparam>
/// </summary>
public class UserConfiguration<TUser> : IEntityTypeConfiguration<TUser>
    where TUser : QubitlabUser
{
    public virtual void Configure(EntityTypeBuilder<TUser> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.NormalizedEmail)
            .IsUnique()
            .HasDatabaseName("IX_Users_NormalizedEmail");

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.HasMany(u => u.UserRoles)
            .WithOne(ur => (TUser)ur.User)
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// Varsayılan <see cref="QubitlabUser"/> konfigürasyonu.
/// Kendi user tipini kullanmak istersen: <see cref="UserConfiguration{TUser}"/>.
/// </summary>
public sealed class UserConfiguration : UserConfiguration<QubitlabUser> { }
