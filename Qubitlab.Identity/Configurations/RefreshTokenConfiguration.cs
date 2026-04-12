using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Qubitlab.Identity.Entities;

namespace Qubitlab.Identity.Configurations;

public class RefreshTokenConfiguration<TToken> : IEntityTypeConfiguration<TToken>
    where TToken : IdentityRefreshToken
{
    public virtual void Configure(EntityTypeBuilder<TToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.HasIndex(t => t.Token)
            .IsUnique()
            .HasDatabaseName("IX_RefreshTokens_Token");

        builder.Property(t => t.AccessTokenJti)
            .HasMaxLength(100);

        builder.Property(t => t.ExpiresAt).IsRequired();

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Varsayılan <see cref="IdentityRefreshToken"/> konfigürasyonu.</summary>
public sealed class RefreshTokenConfiguration : RefreshTokenConfiguration<IdentityRefreshToken> { }
