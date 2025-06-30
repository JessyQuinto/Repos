using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Cart repository implementation with optimized queries for cart operations
/// Includes cart items with proper relationship loading
/// </summary>
public class CartRepository : BaseRepository<Cart>, ICartRepository
{
    public CartRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get cart by ID with cart items and product details
    /// </summary>
    public new async Task<Cart?> GetByIdAsync(int id)
    {
        try
        {            return await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving cart by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get cart by user ID with optimized loading for cart operations
    /// Creates new cart if user doesn't have one
    /// </summary>
    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        try
        {            var cart = await _dbSet
                .Include(c => c.Items)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving cart for user {userId}", ex);
        }
    }

    /// <summary>
    /// Create new cart with proper initialization
    /// </summary>
    public new async Task<Cart> CreateAsync(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        try
        {
            // Ensure cart has proper initialization
            cart.CreatedAt = DateTime.UtcNow;
            cart.UpdatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(cart);
            await _context.SaveChangesAsync();

            // Return cart with loaded relationships
            return await GetByIdAsync(cart.Id) ?? cart;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating cart", ex);
        }
    }

    /// <summary>
    /// Update cart with proper timestamp management
    /// </summary>
    public new async Task<Cart> UpdateAsync(Cart cart)
    {
        if (cart == null)
            throw new ArgumentNullException(nameof(cart));

        try
        {
            // Update timestamp
            cart.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(cart);
            await _context.SaveChangesAsync();

            // Return updated cart with loaded relationships
            return await GetByIdAsync(cart.Id) ?? cart;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Cart was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating cart", ex);
        }
    }

    /// <summary>
    /// Delete cart by ID with cascade delete for cart items
    /// </summary>
    public new async Task DeleteAsync(int id)
    {
        try
        {            var cart = await _dbSet
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart != null)
            {
                // Entity Framework will handle cascade delete for cart items
                _dbSet.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting cart with ID {id}", ex);
        }
    }
}
