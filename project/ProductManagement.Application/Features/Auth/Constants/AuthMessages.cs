namespace ProductManagement.Application.Features.Auth.Constants;

public static class AuthMessages
{
    public const string UserAlreadyExistsMessage = "Bu e-posta adresi ile zaten bir hesap mevcut.";
    public const string UserNotFoundMessage = "Kullanıcı bulunamadı.";
    public const string InvalidCredentialsMessage = "E-posta veya şifre hatalı.";
    public const string AccountIsDeactivatedMessage = "Hesabınız devre dışı bırakılmıştır.";

    // Validation Messages
    public const string FullNameRequiredMessage = "Full name is required.";
    public const string FullNameMinLengthMessage = "Full name must be at least 2 characters.";
    public const string FullNameMaxLengthMessage = "Full name must not exceed 150 characters.";
    public const string EmailRequiredMessage = "Email is required.";
    public const string EmailInvalidMessage = "Please enter a valid email address.";
    public const string EmailMaxLengthMessage = "Email must not exceed 256 characters.";
    public const string PasswordRequiredMessage = "Password is required.";
    public const string PasswordMinLengthMessage = "Password must be at least 8 characters.";
    public const string PasswordMaxLengthMessage = "Password must not exceed 128 characters.";
    public const string PasswordComplexityMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit and one special character.";
    public const string CityRequiredMessage = "City is required.";
    public const string CityMaxLengthMessage = "City must not exceed 100 characters.";
}
