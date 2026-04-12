using System.Security.Cryptography;
using System.Text;

namespace Qubitlab.Security.Hashing;

/// <summary>
/// PBKDF2 + HMAC-SHA256 tabanlı şifre hashleme.
/// ASP.NET Identity ile aynı algoritma — ekstra NuGet paketi gerekmez.
/// </summary>
public sealed class Pbkdf2HashingService : IHashingService
{
    private const int Iterations  = 310_000;   // NIST önerisi (2023)
    private const int SaltSize    = 16;         // 128 bit
    private const int HashSize     = 32;         // 256 bit

    /// <inheritdoc />
    public string Hash(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plainText),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

        // Format: iterations:salt(base64):hash(base64)
        return $"{Iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    /// <inheritdoc />
    public bool Verify(string plainText, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        try
        {
            var parts = hash.Split(':');
            if (parts.Length != 3) return false;

            var iterations = int.Parse(parts[0]);
            var salt       = Convert.FromBase64String(parts[1]);
            var storedHash = Convert.FromBase64String(parts[2]);

            var computedHash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(plainText),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                storedHash.Length);

            return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
        }
        catch
        {
            return false;
        }
    }
}
