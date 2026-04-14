using Qubitlab.Abstractions.Logging;

namespace Qubitlab.CrossCuttingConcerns.Logging;

/// <summary>
/// <see cref="ICorrelationIdProvider"/>'ın HTTP istek ömrüne bağlı implementasyonu.
/// </summary>
/// <remarks>
/// DI'ya <b>Scoped</b> olarak kayıt edilmeli; her HTTP isteği kendi
/// bağımsız CorrelationId instance'ına sahip olur.
/// <para>
/// <see cref="CorrelationIdMiddleware"/> tarafından istek başında <see cref="Set"/> çağrılır,
/// sonraki tüm servisler <see cref="CorrelationId"/> üzerinden aynı ID'ye erişir.
/// </para>
/// </remarks>
public sealed class HttpCorrelationIdProvider : ICorrelationIdProvider
{
    private string _correlationId = string.Empty;

    /// <summary>Mevcut isteğin CorrelationId değeri. <see cref="Set"/> çağrılmadan önce boş string döner.</summary>
    public string CorrelationId => _correlationId;

    /// <summary>
    /// CorrelationId değerini ayarlar. Yalnızca <see cref="CorrelationIdMiddleware"/> tarafından çağrılmalıdır.
    /// </summary>
    /// <exception cref="ArgumentException">Boş veya null değer verilirse fırlatılır.</exception>
    public void Set(string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);
        _correlationId = correlationId;
    }
}
