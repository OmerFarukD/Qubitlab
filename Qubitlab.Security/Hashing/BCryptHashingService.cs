namespace Qubitlab.Security.Hashing;

/// <summary>
/// BCrypt tabanlı şifre hashleme.
/// workFactor: 12 → ~250ms — brute force'a karşı güvenli, kullanıcı UX'ini etkilemez.
/// </summary>
public sealed class BCryptHashingService : IHashingService
{
    private const int WorkFactor = 12;

    /// <inheritdoc />
    public string Hash(string plainText)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        return BCrypt.Net.BCrypt.HashPassword(plainText, WorkFactor);
    }

    /// <inheritdoc />
    public bool Verify(string plainText, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);

        try
        {
            return BCrypt.Net.BCrypt.Verify(plainText, hash);
        }
        catch
        {
            // Hash formatı geçersizse false döner — exception fırlatmaz
            return false;
        }
    }
}
