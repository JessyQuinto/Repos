namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Service for managing refresh tokens
/// Handles creation, validation, and revocation of refresh tokens
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new refresh token for the specified user
    /// </summary>
    /// <param name="userId">The user ID to create the token for</param>
    /// <returns>A secure refresh token</returns>
    Task<string> CreateRefreshTokenAsync(int userId);
    
    /// <summary>
    /// Validates a refresh token for the specified user
    /// </summary>
    /// <param name="userId">The user ID to validate against</param>
    /// <param name="refreshToken">The refresh token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    
    /// <summary>
    /// Revokes a specific refresh token for the user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="refreshToken">The refresh token to revoke</param>
    Task RevokeRefreshTokenAsync(int userId, string refreshToken);
    
    /// <summary>
    /// Revokes all refresh tokens for the specified user
    /// </summary>
    /// <param name="userId">The user ID</param>
    Task RevokeAllRefreshTokensAsync(int userId);
    
    /// <summary>
    /// Cleans up expired refresh tokens from storage
    /// </summary>
    Task CleanupExpiredTokensAsync();
}
