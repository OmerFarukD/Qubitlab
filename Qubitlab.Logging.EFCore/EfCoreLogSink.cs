using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
/// <b>Neden IServiceScopeFactory?</b><br/>
/// Serilog sinks singleton yaşam döngüsüne sahiptir. DbContext ise Scoped'dır.
/// Singleton'dan doğrudan Scoped bir servis çözülemez.
/// <see cref="IServiceScopeFactory"/> (kendisi Singleton) ile her batch için
/// yeni bir scope oluşturulur ve bu scope içerisinden <c>TContext</c> güvenle
/// çözümlenir. Böylece <c>ICurrentUserService</c> gibi Scoped bağımlılıklar
/// da doğru şekilde enjekte edilir.
/// </para>
/// </remarks>
/// <typeparam name="TContext">
/// Kullanılacak DbContext tipi. <see cref="IHasAppLogs"/> implement etmeli.
/// </typeparam>
public sealed class EfCoreLogSink<TContext> : IBatchedLogEventSink
    where TContext : DbContext, IHasAppLogs
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EfCoreSinkOptions _options;

    public EfCoreLogSink(
        IServiceScopeFactory scopeFactory,
        EfCoreSinkOptions options)
    {
        _scopeFactory = scopeFactory;
        _options      = options;
    }

    public async Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        var batchList = batch.ToList();
        if (batchList.Count == 0) return;

        var filtered = batchList
            .Where(e => e.Level >= _options.MinimumLevel)
            .ToList();

        if (filtered.Count == 0) return;

        for (var attempt = 0; attempt <= _options.RetryCount; attempt++)
        {
            try
            {
                await WriteBatchAsync(filtered);
                return;
            }
            catch (Exception ex) when (attempt < _options.RetryCount)
            {
                await Task.Delay(_options.RetryDelay);
                _ = ex;
            }
            catch
            {
                SelfLog.WriteLine(
                    "[EfCoreLogSink] Batch yazılamadı. {0} kayıt kaybedildi.",
                    filtered.Count);
                return;
            }
        }
    }

    public Task OnEmptyBatchAsync() => Task.CompletedTask;

    private async Task WriteBatchAsync(List<LogEvent> events)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<TContext>();
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

    private static string? GetProperty(LogEvent logEvent, string name)
    {
        if (logEvent.Properties.TryGetValue(name, out var value))
            return value is ScalarValue sv ? sv.Value?.ToString() : value.ToString();
        return null;
    }

    private static string? SerializeProperties(LogEvent logEvent)
    {
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
