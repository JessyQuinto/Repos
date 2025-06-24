using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Service for generating and validating authentication tokens
/// Provides abstraction for token operations in the domain layer
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates an access token for the specified user
    /// </summary>
    /// <param name="user">The user to generate the token for</param>
    /// <returns>A JWT access token</returns>
    string GenerateAccessToken(User user);
    
    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <returns>A secure refresh token</returns>
    string GenerateRefreshToken();
    
    /// <summary>
    /// Validates the provided token
    /// </summary>
    /// <param name="token">The token to validate</param>
    /// <returns>True if the token is valid, false otherwise</returns>
    bool ValidateToken(string token);
}
