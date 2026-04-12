namespace Qubitlab.Security.Hashing;

/// <summary>Desteklenen şifre hashleme algoritmaları.</summary>
public enum HashingAlgorithm
{
    /// <summary>
    /// BCrypt — adaptive work factor, güvenli ve yaygın.
    /// Küçük/orta projeler için önerilen default.
    /// </summary>
    BCrypt = 0,

    /// <summary>
    /// PBKDF2 + HMAC-SHA256 — .NET built-in, ekstra NuGet gerekmez.
    /// ASP.NET Identity'nin kullandığı algoritma.
    /// </summary>
    Pbkdf2 = 1
}
