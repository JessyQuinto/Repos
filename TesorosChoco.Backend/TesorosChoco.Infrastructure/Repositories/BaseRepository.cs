using Microsoft.EntityFrameworkCore;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Base repository providing common CRUD operations with Entity Framework Core
/// Implements generic repository pattern with proper error handling and logging
/// </summary>
public abstract class BaseRepository<T> where T : class
{
    protected readonly TesorosChocoDbContext _context;
    protected readonly DbSet<T> _dbSet;

    protected BaseRepository(TesorosChocoDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<T>();
    }

    /// <summary>
    /// Get entity by ID with optimized query execution
    /// </summary>
    protected virtual async Task<T?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id);
        }
        catch (Exception ex)
        {
            // Log error here in production
            throw new InvalidOperationException($"Error retrieving entity by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get all entities with optional include properties
    /// </summary>
    protected virtual async Task<IEnumerable<T>> GetAllAsync(params string[] includeProperties)
    {
        try
        {
            IQueryable<T> query = _dbSet;

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return await query.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all entities", ex);
        }
    }

    /// <summary>
    /// Create new entity with validation
    /// </summary>
    protected virtual async Task<T> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating entity", ex);
        }
    }

    /// <summary>
    /// Update existing entity with optimistic concurrency handling
    /// </summary>
    protected virtual async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Entity was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating entity", ex);
        }
    }    /// <summary>
    /// Delete entity by ID with existence validation
    /// </summary>
    public virtual async Task DeleteAsync(int id)
    {
        try
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting entity with ID {id}", ex);
        }
    }

    /// <summary>
    /// Check if entity exists by ID
    /// </summary>
    protected virtual async Task<bool> ExistsAsync(int id)
    {
        try
        {
            return await _dbSet.FindAsync(id) != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error checking entity existence for ID {id}", ex);
        }
    }
}
