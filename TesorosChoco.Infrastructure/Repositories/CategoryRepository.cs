using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Category repository with slug-based lookup and hierarchy management
/// Optimized for category navigation and product filtering
/// </summary>
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get category by ID with basic information
    /// </summary>
    public new async Task<Category?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving category by ID {id}", ex);
        }
    }

    /// <summary>
    /// Get category by slug for SEO-friendly URLs
    /// </summary>
    public async Task<Category?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug cannot be null or empty", nameof(slug));

        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(c => c.Slug == slug.ToLowerInvariant());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving category by slug {slug}", ex);
        }
    }

    /// <summary>
    /// Get all categories ordered by name
    /// </summary>
    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all categories", ex);
        }
    }

    /// <summary>
    /// Add new category with slug generation and validation
    /// </summary>
    public async Task AddAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        try
        {
            // Generate slug if not provided
            if (string.IsNullOrWhiteSpace(category.Slug))
            {
                category.Slug = GenerateSlug(category.Name);
            }

            // Validate slug uniqueness
            var existingCategory = await GetBySlugAsync(category.Slug);
            if (existingCategory != null)
            {
                throw new InvalidOperationException("Category with this slug already exists");
            }

            // Set timestamps
            category.CreatedAt = DateTime.UtcNow;
            category.UpdatedAt = DateTime.UtcNow;

            await _dbSet.AddAsync(category);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error adding category", ex);
        }
    }

    /// <summary>
    /// Update category with timestamp management
    /// </summary>
    public new async Task UpdateAsync(Category category)
    {
        if (category == null)
            throw new ArgumentNullException(nameof(category));

        try
        {
            // Update timestamp
            category.UpdatedAt = DateTime.UtcNow;

            _dbSet.Update(category);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Category was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating category", ex);
        }
    }

    /// <summary>
    /// Delete category by ID with validation
    /// Checks for related products before deletion
    /// </summary>
    public new async Task DeleteAsync(int id)
    {
        try
        {
            var category = await _dbSet.FindAsync(id);
            if (category != null)
            {
                // Check if category has associated products
                var hasProducts = await _context.Products
                    .AnyAsync(p => p.CategoryId == id);

                if (hasProducts)
                {
                    throw new InvalidOperationException("Cannot delete category that has associated products");
                }

                _dbSet.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException($"Error deleting category with ID {id}", ex);
        }
    }

    /// <summary>
    /// Generate URL-friendly slug from category name
    /// </summary>
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("ñ", "n")
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ü", "u")
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate(string.Empty, (current, c) => current + c);
    }
}
