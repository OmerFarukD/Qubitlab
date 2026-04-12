using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Configurations;

public class UserRoleConfiguration<TUserRole> : IEntityTypeConfiguration<TUserRole>
    where TUserRole : QubitlabUserRole
{
    public virtual void Configure(EntityTypeBuilder<TUserRole> builder)
    {
        builder.ToTable("UserRoles");

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.Property(ur => ur.AssignedAt).IsRequired();

        builder.Property(ur => ur.AssignedBy).HasMaxLength(256);
    }
}

/// <summary>Varsayılan <see cref="QubitlabUserRole"/> konfigürasyonu.</summary>
public sealed class UserRoleConfiguration : UserRoleConfiguration<QubitlabUserRole> { }
