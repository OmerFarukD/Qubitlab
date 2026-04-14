using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Persistence.EFCore.Logging;

/// <summary>
/// <see cref="AppLogEntry"/> tablosunun EF Core model konfigürasyonu.
/// </summary>
public static class AppLogModelBuilderExtensions
{
    /// <summary>
    /// <c>__AppLogs</c> tablosunu ModelBuilder'a ekler ve yapılandırır.
    /// </summary>
    /// <remarks>
    /// <para>
    /// OnModelCreating içinde çağrılmalıdır:
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder builder)
    /// {
    ///     base.OnModelCreating(builder);
    ///     builder.ConfigureAppLogs();
    /// }
    /// </code>
    /// </para>
    /// <para>
    /// Tablo adı <c>__AppLogs</c> (çift alt çizgi) kullanılır —
    /// sistem tablolarını kullanıcı tablolarından ayırt etmek için.
    /// </para>
    /// </remarks>
    public static ModelBuilder ConfigureAppLogs(this ModelBuilder builder)
    {
        builder.Entity<AppLogEntry>(b =>
        {
            b.ToTable("__AppLogs");

            b.HasKey(x => x.Id);

            // long Id → veritabanı tarafından otomatik artar
            b.Property(x => x.Id)
             .ValueGeneratedOnAdd();

            b.Property(x => x.Level)
             .HasMaxLength(20)
             .IsRequired();

            b.Property(x => x.Message)
             .HasMaxLength(4000)
             .IsRequired();

            b.Property(x => x.Exception)
             .HasMaxLength(8000);

            b.Property(x => x.CorrelationId)
             .HasMaxLength(64);

            b.Property(x => x.UserId)
             .HasMaxLength(128);

            b.Property(x => x.MachineName)
             .HasMaxLength(256);

            // Properties alanı — tüm Serilog property'leri JSON olarak
            b.Property(x => x.Properties)
             .HasColumnType("nvarchar(max)"); // PostgreSQL'de "text" kullanılır

            b.Property(x => x.Timestamp)
             .IsRequired();

            // Sık sorgulanan alanlar için index
            b.HasIndex(x => x.Timestamp);
            b.HasIndex(x => x.Level);
            b.HasIndex(x => x.CorrelationId);
        });

        return builder;
    }
}
