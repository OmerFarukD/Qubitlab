using Qubitlab.Abstractions.Security;
using Serilog.Core;
using Serilog.Events;

namespace Qubitlab.Logging.Serilog.Enrichers;

/// <summary>
/// Her log event'e kimliği doğrulanmış kullanıcı bilgilerini ekler.
/// </summary>
/// <remarks>
/// <see cref="ICurrentUserService"/> üzerinden kullanıcı bilgilerine erişir.
/// Eklenen property'ler:
/// <list type="bullet">
///   <item><c>UserId</c> — kullanıcının benzersiz kimliği</item>
///   <item><c>UserEmail</c> — kullanıcının e-posta adresi</item>
/// </list>
/// Kullanıcı oturum açmamışsa hiçbir property eklenmez.
/// </remarks>
internal sealed class CurrentUserEnricher : ILogEventEnricher
{
    private readonly ICurrentUserService _currentUser;

    public CurrentUserEnricher(ICurrentUserService currentUser)
        => _currentUser = currentUser;

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (!_currentUser.IsAuthenticated)
            return;

        if (_currentUser.UserId is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserId", _currentUser.UserId));
        }

        if (_currentUser.Email is not null)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("UserEmail", _currentUser.Email));
        }
    }
}
