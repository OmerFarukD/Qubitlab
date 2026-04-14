using System.Text.Json;
using System.Text.RegularExpressions;
using Qubitlab.Abstractions.Logging;

namespace Qubitlab.Logging.Serilog;

/// <summary>
/// Hassas verileri maskeleyen varsayılan implementasyon.
/// </summary>
/// <remarks>
/// Desteklenen maskeleme stratejileri alan adına göre belirlenir:
/// <list type="bullet">
///   <item><b>Password / PasswordHash / Secret / ApiKey</b> → tamamen gizle: <c>[REDACTED]</c></item>
///   <item><b>Token / AccessToken / RefreshToken</b> → başı + sonu göster: <c>eyJ...abc</c></item>
///   <item><b>Email</b> → kısmi maskele: <c>j***@example.com</c></item>
///   <item><b>CreditCard / CardNumber / Pan</b> → son 4 hane: <c>**** **** **** 1234</c></item>
///   <item><b>Phone / PhoneNumber</b> → son 3 hane: <c>***-***-567</c></item>
///   <item>Diğerleri → tamamen gizle: <c>[REDACTED]</c></item>
/// </list>
/// </remarks>
public sealed class SensitiveDataMasker : ISensitiveDataMasker
{
    private static readonly HashSet<string> _fullRedact = new(StringComparer.OrdinalIgnoreCase)
    {
        "Password", "PasswordHash", "HashedPassword",
        "Secret", "ClientSecret", "ApiKey", "PrivateKey"
    };

    private static readonly HashSet<string> _tokenFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Token", "AccessToken", "RefreshToken", "IdToken", "BearerToken"
    };

    private static readonly HashSet<string> _emailFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "EmailAddress", "Mail"
    };

    private static readonly HashSet<string> _cardFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "CreditCard", "CardNumber", "Pan", "CardNo"
    };

    private static readonly HashSet<string> _phoneFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "Phone", "PhoneNumber", "Mobile", "Gsm"
    };

    /// <inheritdoc/>
    public string Mask(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value)) return value;

        if (_fullRedact.Contains(fieldName))
            return "[REDACTED]";

        if (_tokenFields.Contains(fieldName))
            return MaskToken(value);

        if (_emailFields.Contains(fieldName))
            return MaskEmail(value);

        if (_cardFields.Contains(fieldName))
            return MaskCard(value);

        if (_phoneFields.Contains(fieldName))
            return MaskPhone(value);

        return "[REDACTED]"; // bilinmeyen hassas alan → tamamen gizle
    }

    /// <inheritdoc/>
    public object? MaskObject(object? value, IEnumerable<string> sensitiveFields)
    {
        if (value is null) return null;

        var fieldSet = new HashSet<string>(sensitiveFields, StringComparer.OrdinalIgnoreCase);
        if (!fieldSet.Any()) return value;

        // JSON serialize → property'leri maskele → anonim obje döndür
        try
        {
            var json = JsonSerializer.Serialize(value);
            using var doc = JsonDocument.Parse(json);

            var dict = new Dictionary<string, object?>();
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (fieldSet.Contains(prop.Name))
                    dict[prop.Name] = Mask(prop.Value.ToString(), prop.Name);
                else
                    dict[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString(),
                        JsonValueKind.Number => prop.Value.GetDecimal(),
                        JsonValueKind.True   => true,
                        JsonValueKind.False  => false,
                        JsonValueKind.Null   => null,
                        _                    => prop.Value.ToString()
                    };
            }
            return dict;
        }
        catch
        {
            return "[SERIALIZATION_FAILED]";
        }
    }

    // ── Maskeleme stratejileri ───────────────────────────────────

    /// <summary>JWT / opaque token: ilk 8 + "..." + son 4 karakter.</summary>
    private static string MaskToken(string value)
    {
        if (value.Length <= 12) return "***";
        return $"{value[..8]}...{value[^4..]}";
    }

    /// <summary>E-posta: <c>john.doe@example.com</c> → <c>j***@example.com</c></summary>
    private static string MaskEmail(string value)
    {
        var atIndex = value.IndexOf('@');
        if (atIndex <= 0) return "[REDACTED]";

        var local = value[..atIndex];
        var domain = value[atIndex..];
        var masked = local.Length <= 1 ? "***" : $"{local[0]}***";
        return $"{masked}{domain}";
    }

    /// <summary>Kart numarası: son 4 hane görünür, geri kalan '*'.</summary>
    private static string MaskCard(string value)
    {
        var digits = Regex.Replace(value, @"\D", "");
        if (digits.Length < 4) return "[REDACTED]";
        return $"**** **** **** {digits[^4..]}";
    }

    /// <summary>Telefon: son 3 hane görünür, geri kalan '*'.</summary>
    private static string MaskPhone(string value)
    {
        var digits = Regex.Replace(value, @"\D", "");
        if (digits.Length < 3) return "[REDACTED]";
        var visible = digits[^3..];
        var hidden  = new string('*', digits.Length - 3);
        return $"{hidden}{visible}";
    }
}
