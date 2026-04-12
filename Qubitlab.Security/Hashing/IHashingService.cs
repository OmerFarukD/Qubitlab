namespace Qubitlab.Security.Hashing;

/// <summary>Şifre hashleme ve doğrulama kontratı.</summary>
public interface IHashingService
{
    /// <summary>Plain text şifreyi hashler.</summary>
    string Hash(string plainText);

    /// <summary>Plain text şifrenin hash ile eşleşip eşleşmediğini doğrular.</summary>
    bool Verify(string plainText, string hash);
}
