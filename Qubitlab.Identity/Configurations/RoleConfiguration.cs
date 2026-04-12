using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Configurations;

public class RoleConfiguration<TRole> : IEntityTypeConfiguration<TRole>
    where TRole : QubitlabRole
{
    public virtual void Configure(EntityTypeBuilder<TRole> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.NormalizedName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(r => r.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_Roles_NormalizedName");

        builder.Property(r => r.Description)
            .HasMaxLength(500);
    }
}

/// <summary>Varsayılan <see cref="QubitlabRole"/> konfigürasyonu.</summary>
public sealed class RoleConfiguration : RoleConfiguration<QubitlabRole> { }
