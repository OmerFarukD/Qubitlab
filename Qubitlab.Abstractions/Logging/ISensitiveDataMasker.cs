namespace Qubitlab.Abstractions.Logging;

/// <summary>
/// Log'a yazılmadan önce hassas verileri maskeleyen servis kontratı.
/// </summary>
/// <remarks>
/// Kullanım senaryoları:
/// <list type="bullet">
///   <item>Password, PasswordHash alanlarını tamamen gizle</item>
///   <item>Kredi kartı numarasını kısmi maskele: <c>**** **** **** 1234</c></item>
///   <item>E-posta adresini kısmi maskele: <c>j***@example.com</c></item>
///   <item>JWT token'ı kısaltılmış göster: <c>eyJ...abc</c></item>
/// </list>
/// </remarks>
public interface ISensitiveDataMasker
{
    /// <summary>
    /// Verilen string değeri maskeleme kurallarına göre dönüştürür.
    /// </summary>
    /// <param name="value">Maskelenecek ham değer.</param>
    /// <param name="fieldName">
    /// Hangi maskeleme stratejisinin uygulanacağını belirlemek için alan adı.
    /// Örn: "Password" → tamamen maskele, "Email" → kısmi maskele.
    /// </param>
    string Mask(string value, string fieldName);

    /// <summary>
    /// Bir objenin tüm hassas property'lerini maskeler ve
    /// log'a güvenli yazılabilir anonim obje döndürür.
    /// </summary>
    object? MaskObject(object? value, IEnumerable<string> sensitiveFields);
}
