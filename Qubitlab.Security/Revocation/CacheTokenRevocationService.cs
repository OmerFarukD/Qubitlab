using Qubitlab.Abstractions.Caching;

namespace Qubitlab.Security.Revocation;

/// <summary>
/// <see cref="ICacheService"/> tabanlı token blacklist implementasyonu.
/// Token'ın kalan ömrü kadar cache'de tutulur — expire olunca otomatik silinir.
/// InMemory, Redis veya Hybrid ile çalışır.
/// </summary>
public sealed class CacheTokenRevocationService : ITokenRevocationService
{
    private readonly ICacheService _cache;

    // Cache key prefix — diğer key'lerle çakışmayı önler
    private const string Prefix = "revoked_jti:";

    public CacheTokenRevocationService(ICacheService cache)
    {
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task RevokeAsync(
        string jti,
        TimeSpan tokenRemainingLifetime,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jti);

        if (tokenRemainingLifetime <= TimeSpan.Zero)
            return;  // Zaten süresi dolmuş, eklemeye gerek yok

        // Cache'e yaz — değer önemsiz, key'in var olması yeterli
        await _cache.SetAsync(
            key:        $"{Prefix}{jti}",
            value:      true,
            expiration: tokenRemainingLifetime,
            cancellationToken: ct
        );
    }

    /// <inheritdoc />
    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jti);

        return await _cache.ExistsAsync($"{Prefix}{jti}", ct);
    }
}
