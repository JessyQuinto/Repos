using TesorosChoco.Domain.Entities;
using TesorosChoco.Application.DTOs.Parameters;

namespace TesorosChoco.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetFeaturedAsync();
    Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);
    Task<IEnumerable<Product>> GetByProducerIdAsync(int producerId);
    Task<(IEnumerable<Product> Products, int Total)> SearchAsync(ProductSearchParameters parameters);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task DeleteAsync(int id);
}
