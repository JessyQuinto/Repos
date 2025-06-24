namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Service for password hashing and verification
/// Provides secure password operations using industry-standard algorithms
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashes a password using a secure algorithm
    /// </summary>
    /// <param name="password">The plain text password to hash</param>
    /// <returns>The hashed password</returns>
    string HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against its hash
    /// </summary>
    /// <param name="password">The plain text password to verify</param>
    /// <param name="hash">The hashed password to verify against</param>
    /// <returns>True if the password matches the hash, false otherwise</returns>
    bool VerifyPassword(string password, string hash);
    
    /// <summary>
    /// Checks if a password hash needs to be rehashed (for security upgrades)
    /// </summary>
    /// <param name="hash">The password hash to check</param>
    /// <returns>True if the hash needs to be updated, false otherwise</returns>
    bool NeedsRehashing(string hash);
}
