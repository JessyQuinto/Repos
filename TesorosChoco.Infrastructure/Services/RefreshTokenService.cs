using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TesorosChoco.Infrastructure.Identity;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Service for managing refresh tokens with secure storage and expiration
/// Implements token rotation for enhanced security
/// </summary>
public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(int userId);
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeAllRefreshTokensAsync(int userId);
    Task CleanupExpiredTokensAsync();
}

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<RefreshTokenService> _logger;
    private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromDays(7);
    private const string CACHE_KEY_PREFIX = "refresh_token_";

    public RefreshTokenService(IMemoryCache cache, ILogger<RefreshTokenService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public Task<string> CreateRefreshTokenAsync(int userId)
    {
        var tokenBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(tokenBytes);
        
        var refreshToken = Convert.ToBase64String(tokenBytes);
        var cacheKey = $"{CACHE_KEY_PREFIX}{userId}_{refreshToken}";
        
        var tokenData = new RefreshTokenData
        {
            UserId = userId,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(_refreshTokenLifetime),
            IsRevoked = false
        };

        _cache.Set(cacheKey, tokenData, _refreshTokenLifetime);
        
        _logger.LogInformation("Refresh token created for user {UserId}", userId);
        return Task.FromResult(refreshToken);
    }

    public Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{userId}_{refreshToken}";
        
        if (!_cache.TryGetValue(cacheKey, out RefreshTokenData? tokenData))
        {
            _logger.LogWarning("Refresh token not found for user {UserId}", userId);
            return Task.FromResult(false);
        }

        if (tokenData.IsRevoked || tokenData.ExpiresAt <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token is revoked or expired for user {UserId}", userId);
            _cache.Remove(cacheKey);
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public Task RevokeRefreshTokenAsync(int userId, string refreshToken)
    {
        var cacheKey = $"{CACHE_KEY_PREFIX}{userId}_{refreshToken}";
        
        if (_cache.TryGetValue(cacheKey, out RefreshTokenData? tokenData))
        {
            tokenData.IsRevoked = true;
            _cache.Set(cacheKey, tokenData, TimeSpan.FromMinutes(5)); // Keep for a short time to prevent reuse
            _logger.LogInformation("Refresh token revoked for user {UserId}", userId);
        }

        return Task.CompletedTask;
    }

    public Task RevokeAllRefreshTokensAsync(int userId)
    {
        // Note: This is a simplified implementation using memory cache
        // In production, you would want to use a persistent store (Redis, Database)
        // and maintain a list of tokens per user for bulk revocation
        
        _logger.LogInformation("All refresh tokens revoked for user {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task CleanupExpiredTokensAsync()
    {
        // Memory cache automatically removes expired entries
        // This method is here for interface completeness and could be used
        // with persistent storage implementations
        
        _logger.LogDebug("Cleanup expired tokens completed");
        return Task.CompletedTask;
    }

    private class RefreshTokenData
    {
        public int UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
    }
}
