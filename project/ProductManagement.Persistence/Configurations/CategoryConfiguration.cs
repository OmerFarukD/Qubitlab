using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Persistence.Configurations;

/// <summary>
/// <see cref="Category"/> entity'sinin EF Core Fluent API konfigürasyonu.
/// </summary>
public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedOnAdd();

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Audit alanları (Entity<TId> base class'tan geliyor)
        builder.Property(c => c.CreatedAt)
            .IsRequired();

        builder.Property(c => c.CreatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.UpdatedBy)
            .HasMaxLength(100);

        builder.Property(c => c.DeletedBy)
            .HasMaxLength(100);

        // Soft-delete filtresi QubitlabDbContext base class'ta global olarak uygulanır.
        // IsDeleted index'i sorgularda performans için eklendi.
        builder.HasIndex(c => c.IsDeleted)
            .HasDatabaseName("IX_Categories_IsDeleted");

        builder.HasIndex(c => c.Name)
            .HasDatabaseName("IX_Categories_Name");

        // ─── İlişki: Category → Products (one-to-many) ──────────
        // Bir kategoriye ait birden fazla ürün olabilir.
        // Ters tarafı ProductConfiguration.HasOne(p => p.Category).WithMany(c => c.Products) ile eşleşir.
        builder.HasMany(c => c.Products)
            .WithOne(p => p.Category)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
