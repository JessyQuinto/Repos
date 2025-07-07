using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Product repository implementation with optimized queries and comprehensive search functionality
/// Follows Azure best practices for data access patterns
/// </summary>
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    public new async Task<Product?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving product by ID {id}", ex);
        }
    }

    public async Task<Product?> GetBySlugAsync(string slug)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving product by slug {slug}", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Where(p => p.Status == Domain.Enums.ProductStatus.Active) // Solo productos activos
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all products", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetFeaturedAsync()
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Where(p => p.Featured && p.Status == Domain.Enums.ProductStatus.Active) // Solo activos y destacados
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving featured products", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products by category {categoryId}", ex);
        }
    }

    public async Task<IEnumerable<Product>> GetByProducerIdAsync(int producerId)
    {
        try
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .Where(p => p.ProducerId == producerId)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products by producer {producerId}", ex);
        }
    }

    public async Task<(IEnumerable<Product> Products, int Total)> SearchProductsAsync(
        string? searchTerm = null, 
        int? categoryId = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        int? producerId = null, 
        bool? featured = null, 
        int limit = 10, 
        int offset = 0)
    {
        try
        {
            var query = _dbSet
                .Include(p => p.Category)
                .Include(p => p.Producer)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || 
                                        p.Description.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (producerId.HasValue)
            {
                query = query.Where(p => p.ProducerId == producerId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.CurrentPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.CurrentPrice <= maxPrice.Value);
            }

            if (featured.HasValue)
            {
                query = query.Where(p => p.Featured == featured.Value);
            }

            // Get total count before pagination
            var total = await query.CountAsync();

            // Apply pagination with offset
            var products = await query
                .OrderBy(p => p.Name) // Default sorting
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            return (products, total);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error searching products with advanced filters", ex);
        }
    }

    public new async Task<Product> CreateAsync(Product product)
    {
        try
        {
            _dbSet.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating product", ex);
        }
    }

    public new async Task<Product> UpdateAsync(Product product)
    {
        try
        {
            _dbSet.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating product {product.Id}", ex);
        }
    }

    public new async Task DeleteAsync(int id)
    {
        try
        {
            var product = await GetByIdAsync(id);
            if (product != null)
            {
                _dbSet.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting product {id}", ex);
        }
    }
}