using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// User repository with authentication and profile management capabilities
/// Includes secure user lookup and token management
/// </summary>
public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get user by ID with profile information
    /// </summary>
    public new async Task<User?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get user by email for authentication purposes
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLowerInvariant());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user by email {email}", ex);
        }
    }

    /// <summary>
    /// Get user by refresh token for token refresh operations
    /// </summary>
    public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be null or empty", nameof(refreshToken));

        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && 
                                         u.RefreshTokenExpiryTime > DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving user by refresh token", ex);
        }
    }

    /// <summary>
    /// Get all users with basic information
    /// </summary>
    public async Task<IEnumerable<User>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .OrderBy(u => u.Email)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all users", ex);
        }
    }

    /// <summary>
    /// Add new user with validation and security considerations
    /// </summary>
    public async Task AddAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            // Validate email uniqueness
            var existingUser = await GetByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("User with this email already exists");
            }

            // Ensure proper initialization
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            user.Email = user.Email.ToLowerInvariant();

            await _dbSet.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error adding user", ex);
        }
    }

    /// <summary>
    /// Update user with timestamp management
    /// </summary>
    public new async Task UpdateAsync(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            // Update timestamp
            user.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("User was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating user", ex);
        }
    }

    /// <summary>
    /// Delete user by ID with validation
    /// Consider soft delete for audit purposes in production
    /// </summary>
    public new async Task DeleteAsync(int id)
    {
        try
        {
            var user = await _dbSet.FindAsync(id);
            if (user != null)
            {
                // In production, consider checking for related data (orders, etc.)
                // and implementing soft delete instead of hard delete
                _dbSet.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting user with ID {id}", ex);
        }
    }
}
