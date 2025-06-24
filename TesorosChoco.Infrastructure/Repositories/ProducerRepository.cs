using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Producer repository with producer management and featured producer capabilities
/// Optimized for producer catalog and product associations
/// </summary>
public class ProducerRepository : BaseRepository<Producer>, IProducerRepository
{
    public ProducerRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get producer by ID with basic information
    /// </summary>
    public new async Task<Producer?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving producer by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get all producers ordered by name
    /// </summary>
    public async Task<IEnumerable<Producer>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all producers", ex);
        }
    }

    /// <summary>
    /// Get featured producers for homepage or special sections
    /// </summary>
    public async Task<IEnumerable<Producer>> GetFeaturedAsync()
    {
        try
        {
            return await _dbSet
                .Where(p => p.Featured)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving featured producers", ex);
        }
    }

    /// <summary>
    /// Create new producer with validation and initialization
    /// </summary>
    public new async Task<Producer> CreateAsync(Producer producer)
    {
        if (producer == null)
            throw new ArgumentNullException(nameof(producer));

        try
        {
            // Set timestamps
            producer.CreatedAt = DateTime.UtcNow;
            producer.UpdatedAt = DateTime.UtcNow;

            // Validate name uniqueness
            var existingProducer = await _dbSet
                .FirstOrDefaultAsync(p => p.Name.ToLower() == producer.Name.ToLowerInvariant());
            
            if (existingProducer != null)
            {
                throw new InvalidOperationException("Producer with this name already exists");
            }

            await _dbSet.AddAsync(producer);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(producer.Id) ?? producer;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating producer", ex);
        }
    }

    /// <summary>
    /// Update producer with timestamp management
    /// </summary>
    public new async Task<Producer> UpdateAsync(Producer producer)
    {
        if (producer == null)
            throw new ArgumentNullException(nameof(producer));

        try
        {
            // Update timestamp
            producer.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(producer);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(producer.Id) ?? producer;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Producer was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating producer", ex);
        }
    }

    /// <summary>
    /// Delete producer by ID with validation
    /// Checks for related products before deletion
    /// </summary>
    public new async Task DeleteAsync(int id)
    {
        try
        {
            var producer = await _dbSet.FindAsync(id);
            if (producer != null)
            {
                // Check if producer has associated products
                var hasProducts = await _context.Products
                    .AnyAsync(p => p.ProducerId == id);

                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete producer that has associated products");
                }

                _dbSet.Remove(producer);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting producer with ID {id}", ex);
        }
    }
}
