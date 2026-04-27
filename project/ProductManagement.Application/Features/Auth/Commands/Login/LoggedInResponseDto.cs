namespace ProductManagement.Application.Features.Auth.Commands.Login;

public sealed class LoggedInResponseDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime AccessTokenExpiresAt { get; set; }
}
