using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Persistence.EFCore.Logging;

/// <summary>
/// <see cref="AppLogEntry"/> tablosuna sahip olduğunu bildiren DbContext sözleşmesi.
/// </summary>
/// <remarks>
/// Kullanıcının kendi DbContext'i bu interface'i implement etmelidir:
/// <code>
/// public class AppDbContext : QubitlabDbContext&lt;AppDbContext&gt;, IHasAppLogs
/// {
///     public DbSet&lt;AppLogEntry&gt; AppLogs =&gt; Set&lt;AppLogEntry&gt;();
/// }
/// </code>
/// <b>EF Core migration notu:</b> <see cref="AppLogEntry"/> tablosunu migration'a dahil etmek için
/// OnModelCreating'de <c>ConfigureAppLogs(modelBuilder)</c> metodunu çağırın:
/// <code>
/// protected override void OnModelCreating(ModelBuilder builder)
/// {
///     base.OnModelCreating(builder);
///     builder.ConfigureAppLogs();  // __AppLogs tablosunu yapılandırır
/// }
/// </code>
/// </remarks>
public interface IHasAppLogs
{
    DbSet<AppLogEntry> AppLogs { get; }
}
