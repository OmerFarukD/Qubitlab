using Microsoft.AspNetCore.Http;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.CrossCuttingConcerns.Logging;

/// <summary>
/// Her HTTP isteği için CorrelationId yönetimi sağlar.
/// </summary>
/// <remarks>
/// Davranış:
/// <list type="number">
///   <item>
///     İstemci <c>X-Correlation-Id</c> header gönderiyorsa onu kullanır
///     (microservice zincirinde izleme sürekliliği).
///   </item>
///   <item>
///     Header yoksa yeni bir GUID üretir.
///   </item>
///   <item>
///     <see cref="ICorrelationIdProvider.Set"/> ile mevcut istek scope'una enjekte eder.
///     Bu sayede <c>ICorrelationIdProvider</c> inject eden her servis aynı ID'yi görür.
///   </item>
///   <item>
///     <c>X-Correlation-Id</c> response header'ına da ekler — istemci takip edebilir.
///   </item>
/// </list>
///
/// Serilog entegrasyonu: <c>Qubitlab.Logging.Serilog</c> paketi yüklüyse
/// <c>CorrelationIdEnricher</c> her log event'e CorrelationId'yi otomatik ekler.
///
/// <b>Pipeline sırası:</b> Bu middleware <b>ilk sırada</b> olmalıdır,
/// <c>ExceptionMiddleware</c>'den bile önce.
/// </remarks>
public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    /// <summary>Header adı. RFC 7240 uyumlu özel X- header.</summary>
    public const string HeaderName = "X-Correlation-Id";

    public async Task InvokeAsync(HttpContext context, ICorrelationIdProvider provider)
    {
        // 1) CorrelationId belirle
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
                            ?? Guid.NewGuid().ToString("N"); // kısa, tire yok

        // 2) DI scope'a enjekte et
        provider.Set(correlationId);

        // 3) Response header'a ekle (istemci izleyebilir)
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        await next(context);
    }
}
