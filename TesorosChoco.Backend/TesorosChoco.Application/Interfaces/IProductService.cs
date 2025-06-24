using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Parameters;
using TesorosChoco.Application.DTOs.Requests;

namespace TesorosChoco.Application.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request);
    Task DeleteProductAsync(int id);
    Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParameters parameters);
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<IEnumerable<ProductDto>> GetProductsByProducerAsync(int producerId);
}
