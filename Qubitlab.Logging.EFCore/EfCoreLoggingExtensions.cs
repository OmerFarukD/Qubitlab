using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Qubitlab.Persistence.EFCore.Logging;
using Serilog;
using Serilog.Sinks.PeriodicBatching;

namespace Qubitlab.Logging.EFCore;

/// <summary>
/// EF Core log sink'ini Serilog pipeline'ına ekleyen extension metodları.
/// </summary>
public static class EfCoreLoggingExtensions
{
    /// <summary>
    /// EF Core sink'ini <c>IHostBuilder</c> üzerinden Serilog'a ekler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ön koşul:</b>
    /// <list type="bullet">
    ///   <item>Kullanıcının DbContext'i <see cref="IHasAppLogs"/> implement etmeli</item>
    ///   <item>DI'ya <c>IDbContextFactory&lt;TContext&gt;</c> kayıtlı olmalı
    ///     (<c>services.AddDbContextFactory&lt;AppDbContext&gt;(...)</c>)</item>
    ///   <item>Migration'da <c>builder.ConfigureAppLogs()</c> çağrılmış olmalı</item>
    /// </list>
    /// </para>
    ///
    /// <b>Kullanım (Program.cs):</b>
    /// <code>
    /// builder.Host
    ///     .UseQubitlabSerilog(builder.Configuration)
    ///     .UseQubitlabEfCoreLogging&lt;AppDbContext&gt;();
    ///
    /// // appsettings.json'da ek ayar gerekmez,
    /// // varsayılan: Warning+ seviyeler, 50'li batch, 5sn periyot
    /// </code>
    /// </remarks>
    /// <typeparam name="TContext">
    /// <see cref="IHasAppLogs"/> implement eden DbContext tipi.
    /// </typeparam>
    public static IHostBuilder UseQubitlabEfCoreLogging<TContext>(
        this IHostBuilder hostBuilder,
        Action<EfCoreSinkOptions>? configure = null)
        where TContext : DbContext, IHasAppLogs
    {
        return hostBuilder.UseSerilog((_, serviceProvider, loggerConfig) =>
        {
            // Mevcut konfigürasyona EF Core sink'i ekle
            // (UseQubitlabSerilog'un kurduğu Serilog config'ine ek olarak yazılır)
            AddEfCoreSink<TContext>(loggerConfig, serviceProvider, configure);
        }, writeToProviders: false);
    }

    /// <summary>
    /// Varolan bir <see cref="LoggerConfiguration"/>'a EF Core sink ekler.
    /// </summary>
    /// <remarks>
    /// <c>UseQubitlabSerilog</c>'dan sonra çağrılan ikinci bir <c>UseSerilog</c>
    /// öncekini ezmek yerine birleştirir. Bu metod doğrudan Serilog konfigürasyon
    /// callback'inden çağrılabilir.
    /// </remarks>
    public static LoggerConfiguration WriteToEfCore<TContext>(
        this LoggerConfiguration loggerConfig,
        IServiceProvider serviceProvider,
        Action<EfCoreSinkOptions>? configure = null)
        where TContext : DbContext, IHasAppLogs
    {
        AddEfCoreSink<TContext>(loggerConfig, serviceProvider, configure);
        return loggerConfig;
    }

    // ── Private: sink oluşturma ──────────────────────────────────

    private static void AddEfCoreSink<TContext>(
        LoggerConfiguration loggerConfig,
        IServiceProvider serviceProvider,
        Action<EfCoreSinkOptions>? configure)
        where TContext : DbContext, IHasAppLogs
    {
        var options = new EfCoreSinkOptions();
        configure?.Invoke(options);

        // IDbContextFactory<TContext> DI'dan al
        var factory = serviceProvider.GetRequiredService<IDbContextFactory<TContext>>();

        // Asıl sink
        var sink = new EfCoreLogSink<TContext>(factory, options);

        // PeriodicBatchingSink — sink'i batch'e sarar
        var batchingSink = new PeriodicBatchingSink(sink, new PeriodicBatchingSinkOptions
        {
            BatchSizeLimit = options.BatchSizeLimit,
            Period         = options.Period,
            QueueLimit     = options.QueueLimit
        });

        loggerConfig.WriteTo.Sink(batchingSink, options.MinimumLevel);
    }
}
