using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;
using Qubitlab.Persistence.EFCore.Logging;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Qubitlab.Logging.EFCore;

/// <summary>
/// Serilog log event'lerini EF Core aracılığıyla veritabanına yazan sink.
/// </summary>
/// <remarks>
/// <para>
/// <b>Neden IBatchedLogEventSink?</b><br/>
/// Her log event için ayrı DB çağrısı yapmak (per-event sink) büyük yük oluşturur.
/// <see cref="IBatchedLogEventSink"/> + <c>PeriodicBatchingSink</c> kombinasyonu
/// logları bellekte biriktirip periyodik batch olarak yazar — çok daha verimli.
/// </para>
/// <para>
/// <b>Neden IDbContextFactory?</b><br/>
/// Serilog sinks singleton yaşam döngüsüne sahiptir. DbContext ise Scoped'dur.
/// Scoped DbContext'i singleton'dan çekemeyiz.
/// <see cref="IDbContextFactory{TContext}"/> her batch için yeni, kısa ömürlü
/// bir DbContext instance'ı oluşturur — doğru yaklaşım budur.
/// </para>
/// </remarks>
/// <typeparam name="TContext">
/// Kullanılacak DbContext tipi. <see cref="IHasAppLogs"/> implement etmelidir.
/// </typeparam>
public sealed class EfCoreLogSink<TContext> : IBatchedLogEventSink
    where TContext : DbContext, IHasAppLogs
{
    private readonly IDbContextFactory<TContext> _contextFactory;
    private readonly EfCoreSinkOptions _options;

    public EfCoreLogSink(
        IDbContextFactory<TContext> contextFactory,
        EfCoreSinkOptions options)
    {
        _contextFactory = contextFactory;
        _options        = options;
    }

    /// <summary>
    /// Biriktirilen log batch'ini DB'ye yazar.
    /// Serilog tarafından periyodik olarak çağrılır.
    /// </summary>
    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var batchList = batch.ToList();
        if (batchList.Count == 0) return;

        // Minimum level filtresi
        var filtered = batchList
            .Where(e => e.Level >= _options.MinimumLevel)
            .ToList();

        if (filtered.Count == 0) return;

        for (var attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                await WriteBatchAsync(filtered);
                return; // başarılı → döndür
            }
            catch (Exception ex) when (attempt < _options.RetryCount)
            {
                // Son deneme değilse bekle ve tekrar dene
                // (ana uygulamaya exception sızdırılmaz)
                await Task.Delay(_options.RetryDelay);
                _ = ex; // suppress
            }
            catch
            {
                // Tüm denemeler başarısız → logu yut, uygulamayı etkileme
                // Serilog'un SelfLog mekanizmasını kullan (stderr'e yazar)
                SelfLog.WriteLine(
                    "[EfCoreLogSink] Batch yazılamadı. {0} kayıt kaybedildi.",
                    filtered.Count);
                return;
            }
        }
    }

    /// <summary>
    /// Batch başarısız olduğunda çağrılır. Serilog pipeline'ını bozmaz.
    /// </summary>
    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    // ── Private: asıl yazma işlemi ───────────────────────────────

    private async Task WriteBatchAsync(List<LogEvent> events)
    {
        // Her batch için taze, kısa ömürlü bir DbContext
        await using var context = await _contextFactory.CreateDbContextAsync();

        var entries = events.Select(MapToEntry).ToList();
        await context.AppLogs.AddRangeAsync(entries);
        await context.SaveChangesAsync();
    }

    private AppLogEntry MapToEntry(LogEvent logEvent)
    {
        return new AppLogEntry
        {
            Level         = logEvent.Level.ToString(),
            Message       = logEvent.RenderMessage(),
            Exception     = logEvent.Exception?.ToString(),
            Timestamp     = logEvent.Timestamp.UtcDateTime,
            CorrelationId = GetProperty(logEvent, "CorrelationId"),
            UserId        = GetProperty(logEvent, "UserId"),
            MachineName   = GetProperty(logEvent, "MachineName"),
            Properties    = _options.CaptureProperties
                            ? SerializeProperties(logEvent)
                            : null
        };
    }

    // ── Yardımcı metodlar ────────────────────────────────────────

    /// <summary>Serilog property değerini string olarak çeker.</summary>
    private static string? GetProperty(LogEvent logEvent, string name)
    {
        if (logEvent.Properties.TryGetValue(name, out var value))
            return value is ScalarValue sv ? sv.Value?.ToString() : value.ToString();
        return null;
    }

    /// <summary>
    /// Tüm Serilog property'lerini (bilinen özel alanlar hariç) JSON'a dönüştürür.
    /// </summary>
    private static string? SerializeProperties(LogEvent logEvent)
    {
        // Zaten ayrı kolonda saklanan alanları çıkar — tekrarı önle
        var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CorrelationId", "UserId", "MachineName"
        };

        var props = logEvent.Properties
            .Where(p => !excluded.Contains(p.Key))
            .ToDictionary(
                p => p.Key,
                p => p.Value is ScalarValue sv ? (object?)sv.Value : p.Value.ToString());

        return props.Count > 0
            ? JsonSerializer.Serialize(props)
            : null;
    }
}
