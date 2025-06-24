using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Repository interface for User entity operations
/// Provides data access methods for user management
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by their unique identifier
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByIdAsync(int id);
    
    /// <summary>
    /// Gets a user by their email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email);
    
    /// <summary>
    /// Gets a user by their refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    
    /// <summary>
    /// Gets all users
    /// </summary>
    /// <returns>Collection of all users</returns>
    Task<IEnumerable<User>> GetAllAsync();
    
    /// <summary>
    /// Adds a new user
    /// </summary>
    /// <param name="user">The user to add</param>
    Task AddAsync(User user);
    
    /// <summary>
    /// Updates an existing user
    /// </summary>
    /// <param name="user">The user to update</param>
    Task UpdateAsync(User user);
    
    /// <summary>
    /// Deletes a user by their ID
    /// </summary>
    /// <param name="id">The user ID to delete</param>
    Task DeleteAsync(int id);
}
