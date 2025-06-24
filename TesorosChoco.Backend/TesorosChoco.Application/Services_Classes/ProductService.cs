using Microsoft.Extensions.Logging;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Parameters;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Domain.Entities;
using AutoMapper;

namespace TesorosChoco.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting all products");

            var products = await _productRepository.GetAllAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogInformation("Retrieved {Count} products", productDtos.Count());
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw new Exception("An error occurred while getting products");
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return null;
            }

            return _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
            throw new Exception($"An error occurred while getting product with ID {id}");
        }
    }

    public async Task<ProductDto?> GetProductBySlugAsync(string slug)
    {
        try
        {
            _logger.LogInformation("Getting product with slug: {Slug}", slug);

            var product = await _productRepository.GetBySlugAsync(slug);
            if (product == null)
            {
                _logger.LogWarning("Product with slug {Slug} not found", slug);
                return null;
            }

            return _mapper.Map<ProductDto>(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with slug {Slug}", slug);
            throw new Exception($"An error occurred while getting product with slug '{slug}'");
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", request.Name);

            // Check if product with same slug already exists
            var existingProduct = await _productRepository.GetBySlugAsync(request.Slug);
            if (existingProduct != null)
            {
                _logger.LogWarning("Product creation failed: Product with slug {Slug} already exists", request.Slug);
                throw new InvalidOperationException($"Product with slug '{request.Slug}' already exists");
            }

            var product = new Product
            {
                Name = request.Name,
                Slug = request.Slug,
                Description = request.Description,
                Price = request.Price,
                DiscountedPrice = request.DiscountedPrice,
                Image = request.Image,
                Images = request.Images,
                CategoryId = request.CategoryId,
                ProducerId = request.ProducerId,
                Stock = request.Stock,
                Featured = request.Featured,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _productRepository.AddAsync(product);
            
            var productDto = _mapper.Map<ProductDto>(product);

            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return productDto;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            throw new Exception("An error occurred while creating the product");
        }
    }

    public async Task UpdateProductAsync(UpdateProductRequest request)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", request.Id);

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Product update failed: Product with ID {ProductId} not found", request.Id);
                throw new ArgumentException($"Product with ID {request.Id} not found");
            }

            // Update product properties
            product.Name = request.Name;
            product.Slug = request.Slug;
            product.Description = request.Description;
            product.Price = request.Price;
            product.DiscountedPrice = request.DiscountedPrice;
            product.Image = request.Image;
            product.Images = request.Images;
            product.CategoryId = request.CategoryId;
            product.ProducerId = request.ProducerId;
            product.Stock = request.Stock;
            product.Featured = request.Featured;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            _logger.LogInformation("Product with ID {ProductId} updated successfully", request.Id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", request.Id);
            throw new Exception($"An error occurred while updating product with ID {request.Id}");
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);

            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product deletion failed: Product with ID {ProductId} not found", id);
                throw new ArgumentException($"Product with ID {id} not found");
            }

            await _productRepository.DeleteAsync(id);

            _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            throw new Exception($"An error occurred while deleting product with ID {id}");
        }
    }

    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParameters parameters)
    {
        try
        {
            _logger.LogInformation("Searching products with parameters: {@Parameters}", parameters);

            var (products, total) = await _productRepository.SearchAsync(parameters);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            var result = new PagedResult<ProductDto>
            {
                Items = productDtos,
                Total = total,
                Page = parameters.Page,
                Limit = parameters.Limit
            };

            _logger.LogInformation("Found {Count} products matching search criteria", total);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            throw new Exception("An error occurred while searching products");
        }
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        try
        {
            _logger.LogInformation("Getting featured products");

            var products = await _productRepository.GetFeaturedAsync();
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogInformation("Retrieved {Count} featured products", productDtos.Count());
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured products");
            throw new Exception("An error occurred while getting featured products");
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting products for category: {CategoryId}", categoryId);

            var products = await _productRepository.GetByCategoryAsync(categoryId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogInformation("Retrieved {Count} products for category {CategoryId}", productDtos.Count(), categoryId);
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for category {CategoryId}", categoryId);
            throw new Exception($"An error occurred while getting products for category {categoryId}");
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByProducerAsync(int producerId)
    {
        try
        {
            _logger.LogInformation("Getting products for producer: {ProducerId}", producerId);

            var products = await _productRepository.GetByProducerAsync(producerId);
            var productDtos = _mapper.Map<IEnumerable<ProductDto>>(products);

            _logger.LogInformation("Retrieved {Count} products for producer {ProducerId}", productDtos.Count(), producerId);
            return productDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for producer {ProducerId}", producerId);
            throw new Exception($"An error occurred while getting products for producer {producerId}");
        }
    }
}
