using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Qubitlab.Abstractions.Logging;
using Qubitlab.CrossCuttingConcerns.Logging;

namespace Qubitlab.CrossCuttingConcerns.Extensions;

/// <summary>
/// <c>Qubitlab.CrossCuttingConcerns</c> logging bileşenlerini uygulamaya ekleyen extension metodları.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// <see cref="ICorrelationIdProvider"/> ve ilgili servisleri DI'ya kaydeder.
    /// </summary>
    /// <remarks>
    /// Kayıt edilen servisler:
    /// <list type="bullet">
    ///   <item><see cref="ICorrelationIdProvider"/> → <see cref="HttpCorrelationIdProvider"/> (Scoped)</item>
    /// </list>
    /// </remarks>
    public static IServiceCollection AddQubitlabCorrelation(this IServiceCollection services)
    {
        services.AddSingleton<ICorrelationIdProvider, HttpCorrelationIdProvider>();
        return services;
    }

    /// <summary>
    /// <see cref="CorrelationIdMiddleware"/>'i ASP.NET Core pipeline'ına ekler.
    /// </summary>
    /// <remarks>
    /// <b>Pipeline sırası:</b> Bu metod diğer tüm <c>Use*</c> middleware
    /// çağrılarından <b>önce</b> çağrılmalıdır:
    /// <code>
    /// app.UseQubitlabCorrelation();   // 1. sıra
    /// app.UseQubitlabExceptions();    // 2. sıra
    /// app.UseAuthentication();
    /// app.UseAuthorization();
    /// app.MapControllers();
    /// </code>
    /// </remarks>
    public static IApplicationBuilder UseQubitlabCorrelation(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        return app;
    }
}
