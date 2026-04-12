using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Qubitlab.Security.Jwt;

namespace Qubitlab.Security.RefreshToken;

/// <summary>
/// Refresh token iş mantığı implementasyonu.
/// Kriptografik güvenli rastgele token üretir.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenService(
        IRefreshTokenRepository repository,
        IOptions<JwtSettings> jwtSettings)
    {
        _repository  = repository;
        _jwtSettings = jwtSettings.Value;
    }

    /// <inheritdoc />
    public async Task<RefreshToken> CreateAsync(
        string userId,
        string? accessTokenJti = null,
        CancellationToken ct = default)
    {
        var token = new RefreshToken
        {
            Token          = GenerateSecureToken(),
            UserId         = userId,
            CreatedAt      = DateTime.UtcNow,
            ExpiresAt      = DateTime.UtcNow.Add(_jwtSettings.RefreshTokenExpiration),
            AccessTokenJti = accessTokenJti
        };

        await _repository.AddAsync(token, ct);
        return token;
    }

    /// <inheritdoc />
    public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => await _repository.GetByTokenAsync(token, ct);

    /// <inheritdoc />
    public async Task<RefreshToken> RotateAsync(
        string oldToken,
        string userId,
        string? newAccessTokenJti = null,
        CancellationToken ct = default)
    {
        var existing = await _repository.GetByTokenAsync(oldToken, ct)
            ?? throw new InvalidOperationException("Refresh token bulunamadı.");

        if (existing.IsExpiredOrRevoked)
            throw new InvalidOperationException("Refresh token geçersiz veya süresi dolmuş.");

        // Eski token'ı kullanıldı olarak işaretle (rotation)
        existing.IsUsed = true;
        await _repository.RevokeAsync(oldToken, ct);

        // Yeni token oluştur
        return await CreateAsync(userId, newAccessTokenJti, ct);
    }

    /// <inheritdoc />
    public async Task RevokeAsync(string token, CancellationToken ct = default)
        => await _repository.RevokeAsync(token, ct);

    /// <inheritdoc />
    public async Task RevokeAllAsync(string userId, CancellationToken ct = default)
        => await _repository.RevokeAllByUserIdAsync(userId, ct);

    // ────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────

    /// <summary>
    /// 64 byte kriptografik rastgele token üretir (Base64URL).
    /// SHA-1, GUID gibi tahmin edilebilir yöntemler kullanılmaz.
    /// </summary>
    private static string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');   // URL-safe Base64
    }
}
