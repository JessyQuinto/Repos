using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Parameters;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Product service implementation with comprehensive business logic
/// Follows Azure best practices for error handling and performance
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProducerRepository _producerRepository;
    private readonly IMapper _mapper;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IProducerRepository producerRepository,
        IMapper mapper)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _producerRepository = producerRepository ?? throw new ArgumentNullException(nameof(producerRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all products", ex);
        }
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product != null ? _mapper.Map<ProductDto>(product) : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving product with ID {id}", ex);
        }
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        try
        {
            // Validate that category and producer exist
            var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
            if (category == null)
                throw new ArgumentException($"Category with ID {request.CategoryId} does not exist");

            var producer = await _producerRepository.GetByIdAsync(request.ProducerId);
            if (producer == null)
                throw new ArgumentException($"Producer with ID {request.ProducerId} does not exist");

            var product = _mapper.Map<Product>(request);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            // Generate slug from name
            product.Slug = GenerateSlug(product.Name);

            var createdProduct = await _productRepository.CreateAsync(product);
            
            // Reload with navigation properties
            var productWithDetails = await _productRepository.GetByIdAsync(createdProduct.Id);
            return _mapper.Map<ProductDto>(productWithDetails);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating product", ex);
        }
    }

    public async Task<ProductDto> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        try
        {
            var existingProduct = await _productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                throw new ArgumentException($"Product with ID {id} does not exist");            // Validate category and producer if they are being updated (non-zero values)
            if (request.CategoryId > 0)
            {
                var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
                if (category == null)
                    throw new ArgumentException($"Category with ID {request.CategoryId} does not exist");
            }

            if (request.ProducerId > 0)
            {
                var producer = await _producerRepository.GetByIdAsync(request.ProducerId);
                if (producer == null)
                    throw new ArgumentException($"Producer with ID {request.ProducerId} does not exist");
            }

            // Update properties
            _mapper.Map(request, existingProduct);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Update slug if name changed
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                existingProduct.Slug = GenerateSlug(request.Name);
            }

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            
            // Reload with navigation properties
            var productWithDetails = await _productRepository.GetByIdAsync(updatedProduct.Id);
            return _mapper.Map<ProductDto>(productWithDetails);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating product with ID {id}", ex);
        }
    }

    public async Task DeleteProductAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                throw new ArgumentException($"Product with ID {id} does not exist");

            await _productRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error deleting product with ID {id}", ex);
        }
    }

    public async Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParameters parameters)
    {
        try
        {
            // Convert parameters to repository method parameters
            var (products, total) = await _productRepository.SearchAsync(
                searchTerm: parameters.Q,
                categoryId: parameters.Category,
                producerId: parameters.Producer,
                minPrice: parameters.MinPrice,
                maxPrice: parameters.MaxPrice,
                featured: parameters.Featured,
                page: (parameters.Offset / parameters.Limit) + 1,
                limit: parameters.Limit,
                sortBy: "name",
                sortOrder: "asc"
            );

            var productDtos = _mapper.Map<List<ProductDto>>(products);
            
            return new PagedResult<ProductDto>
            {
                Products = productDtos,
                Total = total,
                Page = (parameters.Offset / parameters.Limit) + 1,
                Limit = parameters.Limit
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error searching products", ex);
        }
    }

    public async Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync()
    {
        try
        {
            var products = await _productRepository.GetFeaturedAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving featured products", ex);
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId)
    {
        try
        {
            var products = await _productRepository.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products for category {categoryId}", ex);
        }
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByProducerAsync(int producerId)
    {
        try
        {
            var products = await _productRepository.GetByProducerIdAsync(producerId);
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving products for producer {producerId}", ex);
        }
    }

    /// <summary>
    /// Generates URL-friendly slug from product name
    /// </summary>
    private static string GenerateSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
            .Replace("ñ", "n")
            .Replace("ü", "u")
            .Replace(".", "")
            .Replace(",", "")
            .Replace("!", "")
            .Replace("?", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("{", "")
            .Replace("}", "")
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .Aggregate(new System.Text.StringBuilder(), (sb, c) => sb.Append(c))
            .ToString()
            .Trim('-');
    }
}
