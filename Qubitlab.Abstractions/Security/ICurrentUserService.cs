namespace Qubitlab.Abstractions.Security;

/// <summary>
/// O anki HTTP isteğini yapan kullanıcının bilgilerini sağlayan servis kontratı.
/// </summary>
/// <remarks>
/// Kullanım senaryoları:
/// <list type="bullet">
///   <item>QubitlabDbContext — CreatedBy, UpdatedBy, DeletedBy audit alanlarını doldurur</item>
///   <item>Authorization logic — kullanıcı kendi kaydını mı değiştiriyor?</item>
///   <item>Tenant isolation — kullanıcının tenant'ına göre veri filtrele</item>
/// </list>
/// </remarks>
public interface ICurrentUserService
{
    /// <summary>Kullanıcı kimlik doğrulaması yaptı mı?</summary>
    bool IsAuthenticated { get; }

    /// <summary>Kullanıcının benzersiz ID'si (NameIdentifier claim). Giriş yapılmamışsa <c>null</c>.</summary>
    string? UserId { get; }

    /// <summary>Kullanıcının e-posta adresi. Giriş yapılmamışsa <c>null</c>.</summary>
    string? Email { get; }

    /// <summary>Kullanıcının adı (Name claim). Giriş yapılmamışsa <c>null</c>.</summary>
    string? Username { get; }

    /// <summary>Kullanıcının rolleri.</summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>Kullanıcının belirtilen rolde olup olmadığını kontrol eder.</summary>
    bool IsInRole(string role);

    /// <summary>Belirtilen claim değerini döner. Yoksa <c>null</c>.</summary>
    string? GetClaim(string claimType);
}
