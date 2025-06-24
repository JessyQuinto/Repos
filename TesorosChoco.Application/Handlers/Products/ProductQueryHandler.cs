using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Handlers.Products;

/// <summary>
/// Handler for product queries
/// </summary>
public class ProductQueryHandler : 
    IRequestHandler<GetAllProductsQuery, IEnumerable<ProductDto>>,
    IRequestHandler<GetProductByIdQuery, ProductDto?>,
    IRequestHandler<GetProductsByCategoryQuery, IEnumerable<ProductDto>>,
    IRequestHandler<GetFeaturedProductsQuery, IEnumerable<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductQueryHandler> _logger;

    public ProductQueryHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<ProductQueryHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting all products with filters: Featured={Featured}, CategoryId={CategoryId}, SearchTerm={SearchTerm}", 
                request.Featured, request.CategoryId, request.SearchTerm);

            var products = await _productRepository.GetAllAsync();
            
            // Apply filters
            if (request.Featured.HasValue)
            {
                products = products.Where(p => p.Featured == request.Featured.Value);
            }
            
            if (request.CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == request.CategoryId.Value);
            }
            
            if (request.ProducerId.HasValue)
            {
                products = products.Where(p => p.ProducerId == request.ProducerId.Value);
            }
            
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLowerInvariant();
                products = products.Where(p => 
                    p.Name.ToLowerInvariant().Contains(searchTerm) ||
                    p.Description.ToLowerInvariant().Contains(searchTerm));
            }

            // Apply pagination
            products = products
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize);

            var result = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _logger.LogInformation("Retrieved {Count} products", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw;
        }
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting product by ID: {ProductId}", request.Id);
            
            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", request.Id);
                return null;
            }

            var result = _mapper.Map<ProductDto>(product);
            _logger.LogInformation("Retrieved product: {ProductName}", product.Name);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}", request.Id);
            throw;
        }
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetProductsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting products by category: {CategoryId}", request.CategoryId);
            
            var products = await _productRepository.GetByCategoryIdAsync(request.CategoryId);
            var result = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _logger.LogInformation("Retrieved {Count} products for category {CategoryId}", result.Count(), request.CategoryId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}", request.CategoryId);
            throw;
        }
    }

    public async Task<IEnumerable<ProductDto>> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting featured products, count: {Count}", request.Count);
            
            var products = await _productRepository.GetFeaturedAsync(request.Count);
            var result = _mapper.Map<IEnumerable<ProductDto>>(products);
            
            _logger.LogInformation("Retrieved {Count} featured products", result.Count());
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving featured products");
            throw;
        }
    }
}
