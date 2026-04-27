using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configurations;

/// <summary>
/// <see cref="Product"/> entity'sinin EF Core Fluent API konfigürasyonu.
/// </summary>
public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Stock)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        // Audit alanları (Entity<TId> base class'tan geliyor)
        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(p => p.DeletedBy)
            .HasMaxLength(100);

        // ─── Foreign Key: Category ────────────────────────────
        builder.Property(p => p.CategoryId)
            .IsRequired();

        // Her iki taraf da navigation property'ye sahip — Category.Products ↔ Product.Category
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Index'ler ────────────────────────────────────────
        builder.HasIndex(p => p.CategoryId)
            .HasDatabaseName("IX_Products_CategoryId");

        builder.HasIndex(p => p.IsDeleted)
            .HasDatabaseName("IX_Products_IsDeleted");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        // Price'a göre sorgular için
        builder.HasIndex(p => p.Price)
            .HasDatabaseName("IX_Products_Price");
    }
}
