using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Order repository with comprehensive order management capabilities
/// Includes order items and related product information
/// </summary>
public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get order by ID with complete order details
    /// </summary>
    public new async Task<Order?> GetByIdAsync(int id)
    {
        try
        {            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving order by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get all orders for a specific user with pagination support
    /// </summary>
    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId)
    {
        try
        {            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving orders for user {userId}", ex);
        }
    }    /// <summary>
    /// Get all orders with filtering and sorting capabilities
    /// </summary>
    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all orders", ex);
        }
    }

    /// <summary>
    /// Create new order with proper initialization and validation
    /// </summary>
    public new async Task<Order> CreateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        try
        {
            // Ensure proper timestamps and initialization
            order.CreatedAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;            // Generate order number if not provided
            // Since Order entity doesn't have OrderNumber, we skip this

            // Validate order items
            if (order.Items == null || !order.Items.Any())
            {
                throw new InvalidOperationException("Order must contain at least one item");
            }

            // Calculate total if not set
            if (order.Total == 0)
            {
                order.Total = order.Items.Sum(oi => oi.Price * oi.Quantity);
            }

            await _dbSet.AddAsync(order);
            await _context.SaveChangesAsync();

            // Return order with loaded relationships
            return await GetByIdAsync(order.Id) ?? order;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating order", ex);
        }
    }

    /// <summary>
    /// Update order with timestamp management and validation
    /// </summary>
    public new async Task<Order> UpdateAsync(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        try
        {
            // Update timestamp
            order.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(order);
            await _context.SaveChangesAsync();

            // Return updated order with loaded relationships
            return await GetByIdAsync(order.Id) ?? order;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Order was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating order", ex);
        }
    }

    /// <summary>
    /// Delete order by ID with validation
    /// Note: In production, consider soft delete for audit purposes
    /// </summary>
    public new async Task DeleteAsync(int id)
    {
        try
        {            var order = await _dbSet
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // Validate that order can be deleted (e.g., not shipped)
                if (order.Status == Domain.Enums.OrderStatus.Shipped || 
                    order.Status == Domain.Enums.OrderStatus.Delivered)
                {
                    throw new InvalidOperationException("Cannot delete shipped or delivered orders");
                }

                // Entity Framework will handle cascade delete for order items
                _dbSet.Remove(order);
                await _context.SaveChangesAsync();
            }        }        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting order with ID {id}", ex);
        }
    }

    /// <summary>
    /// Get orders for a specific user within a date range for business rule validation
    /// </summary>
    public async Task<IEnumerable<Order>> GetOrdersByUserAndDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _dbSet
                .Where(o => o.UserId == userId && 
                           o.CreatedAt >= startDate && 
                           o.CreatedAt < endDate)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving orders for user {userId} in date range", ex);
        }
    }
}
