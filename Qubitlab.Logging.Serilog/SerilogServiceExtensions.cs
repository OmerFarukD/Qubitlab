using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qubitlab.Abstractions.Logging;
using Qubitlab.Logging.Serilog.Enrichers;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace Qubitlab.Logging.Serilog;

/// <summary>
/// <c>Qubitlab.Logging.Serilog</c> paketini uygulamaya ekleyen extension metodları.
/// </summary>
public static class SerilogServiceExtensions
{
    /// <summary>
    /// Serilog tabanlı logging altyapısını DI konteynerine kaydeder.
    /// </summary>
    /// <remarks>
    /// Kayıt edilen servisler:
    /// <list type="bullet">
    ///   <item><see cref="IAppLogger{T}"/> → <see cref="AppLogger{T}"/> (Open Generic)</item>
    ///   <item><see cref="ISensitiveDataMasker"/> → <see cref="SensitiveDataMasker"/></item>
    ///   <item>Enricher'lar: CorrelationId, CurrentUser (servis olarak)</item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Program.cs
    /// builder.Host.UseQubitlabSerilog(builder.Configuration);
    /// builder.Services.AddQubitlabSerilog(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddQubitlabSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SerilogOptions>? configure = null)
    {
        // Options oluştur ve appsettings'ten bind et
        var options = new SerilogOptions();
        configuration.GetSection("Qubitlab:Logging").Bind(options);
        configure?.Invoke(options); // kod tarafında override imkanı

        services.AddSingleton(options);

        // IAppLogger<T> → AppLogger<T> (Open Generic — her T için çalışır)
        services.AddSingleton(typeof(IAppLogger<>), typeof(AppLogger<>));

        // ISensitiveDataMasker
        services.AddSingleton<ISensitiveDataMasker, SensitiveDataMasker>();

        // Enricher'lar — Singleton: IHttpContextAccessor üzerinden per-request okuma yapar
        if (options.EnrichWithCorrelationId)
            services.AddSingleton<CorrelationIdEnricher>();

        if (options.EnrichWithCurrentUser)
            services.AddSingleton<CurrentUserEnricher>();

        return services;
    }

    /// <summary>
    /// Serilog'u <see cref="IHostBuilder"/> pipeline'ına bağlar.
    /// <para>
    /// <b>ÖNEMLI:</b> <c>AddQubitlabSerilog()</c>'dan <b>önce</b> çağrılmalıdır
    /// çünkü Serilog'u MEL provider olarak ayarlar.
    /// </para>
    /// </summary>
    public static IHostBuilder UseQubitlabSerilog(
        this IHostBuilder hostBuilder,
        IConfiguration configuration)
    {
        return hostBuilder.UseSerilog((context, serviceProvider, config) =>
        {
            var options = context.Configuration
                              .GetSection("Qubitlab:Logging")
                              .Get<SerilogOptions>()
                          ?? new SerilogOptions();

            // ── Minimum Level ────────────────────────────────────────
            config
                .MinimumLevel.Is(ToSerilogLevel(options.MinimumLevel))
                .MinimumLevel.Override("Microsoft",
                    ToSerilogLevel(options.MicrosoftMinimumLevel))
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information);

            // ── Enrichment ───────────────────────────────────────────
            config.Enrich.FromLogContext(); // BeginScope / LogContext.PushProperty

            if (options.EnrichWithCorrelationId)
            {
                var enricher = serviceProvider.GetService<CorrelationIdEnricher>();
                if (enricher is not null)
                    config.Enrich.With(enricher);
            }

            if (options.EnrichWithCurrentUser)
            {
                var enricher = serviceProvider.GetService<CurrentUserEnricher>();
                if (enricher is not null)
                    config.Enrich.With(enricher);
            }

            if (options.EnrichWithMachineName)
                config.Enrich.WithMachineName();

            if (options.EnrichWithThreadId)
                config.Enrich.WithThreadId();

            // ── Sink: Console ────────────────────────────────────────
            if (options.WriteToConsole)
            {
                if (options.ConsoleFormat.Equals("CompactJson", StringComparison.OrdinalIgnoreCase))
                    config.WriteTo.Console(new RenderedCompactJsonFormatter());
                else
                    config.WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");
            }

            // ── Sink: File ───────────────────────────────────────────
            if (options.WriteToFile)
            {
                var rollingInterval = Enum.TryParse<RollingInterval>(
                    options.RollingInterval, ignoreCase: true, out var ri)
                    ? ri
                    : RollingInterval.Day;

                config.WriteTo.File(
                    formatter: new CompactJsonFormatter(),
                    path: options.FilePath,
                    rollingInterval: rollingInterval,
                    retainedFileCountLimit: options.RetainedFileCount,
                    shared: true);
            }

            // ── Sink: Seq ────────────────────────────────────────────
            // Not: Seq sink'i için projeye Serilog.Sinks.Seq NuGet paketi eklenmelidir.
            // Şu an temel paket bağımlılıkları arasında yer almıyor.
            // Örnek kullanım (paket eklenince):
            //   config.WriteTo.Seq(options.SeqUrl!, apiKey: options.SeqApiKey);
            if (options.WriteToSeq && !string.IsNullOrWhiteSpace(options.SeqUrl))
            {
                // Seq sink kullanmak için: dotnet add package Serilog.Sinks.Seq
                throw new InvalidOperationException(
                    "Seq sink kullanmak için 'Serilog.Sinks.Seq' NuGet paketi ekleyin " +
                    "ve SerilogServiceExtensions'ı override edin.");
            }
        });
    }

    // ── Yardımcı: MEL LogLevel → Serilog LogEventLevel ──────────
    private static LogEventLevel ToSerilogLevel(LogLevel level) => level switch
    {
        LogLevel.Trace       => LogEventLevel.Verbose,
        LogLevel.Debug       => LogEventLevel.Debug,
        LogLevel.Information => LogEventLevel.Information,
        LogLevel.Warning     => LogEventLevel.Warning,
        LogLevel.Error       => LogEventLevel.Error,
        LogLevel.Critical    => LogEventLevel.Fatal,
        _                    => LogEventLevel.Information
    };
}
