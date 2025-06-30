using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetFeaturedAsync();
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> GetByProducerIdAsync(int producerId);
    Task<(IEnumerable<Product> Products, int Total)> SearchProductsAsync(string? searchTerm = null, int? categoryId = null, decimal? minPrice = null, decimal? maxPrice = null, int? producerId = null, bool? featured = null, int limit = 10, int offset = 0);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task AddAsync(Product product);
    Task DeleteAsync(int id);
}
