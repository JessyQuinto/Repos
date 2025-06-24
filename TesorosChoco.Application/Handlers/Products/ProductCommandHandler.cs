using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using TesorosChoco.Application.Commands.Products;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Handlers.Products;

/// <summary>
/// Handler for product commands
/// </summary>
public class ProductCommandHandler : 
    IRequestHandler<CreateProductCommand, ProductDto>,
    IRequestHandler<UpdateProductCommand, ProductDto?>,
    IRequestHandler<DeleteProductCommand, bool>,
    IRequestHandler<UpdateProductStockCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductCommandHandler> _logger;

    public ProductCommandHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<ProductCommandHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", request.Request.Name);

            // Validate slug uniqueness
            var existingProduct = await _productRepository.GetBySlugAsync(request.Request.Slug);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with slug '{request.Request.Slug}' already exists");
            }

            var product = _mapper.Map<Product>(request.Request);
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.AddAsync(product);
            
            var result = _mapper.Map<ProductDto>(product);
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Request.Name);
            throw;
        }
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating product with ID: {ProductId}", request.Id);

            var existingProduct = await _productRepository.GetByIdAsync(request.Id);
            if (existingProduct == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update", request.Id);
                return null;
            }

            // Check slug uniqueness if slug is being changed
            if (request.Request.Slug != existingProduct.Slug)
            {
                var productWithSlug = await _productRepository.GetBySlugAsync(request.Request.Slug);
                if (productWithSlug != null && productWithSlug.Id != request.Id)
                {
                    throw new InvalidOperationException($"Product with slug '{request.Request.Slug}' already exists");
                }
            }

            _mapper.Map(request.Request, existingProduct);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(existingProduct);
            
            var result = _mapper.Map<ProductDto>(existingProduct);
            _logger.LogInformation("Product updated successfully: {ProductId}", request.Id);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", request.Id);
            throw;
        }
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", request.Id);

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", request.Id);
                return false;
            }

            await _productRepository.DeleteAsync(request.Id);
            
            _logger.LogInformation("Product deleted successfully: {ProductId}", request.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", request.Id);
            throw;
        }
    }

    public async Task<bool> Handle(UpdateProductStockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Updating stock for product {ProductId} to {NewStock}", request.ProductId, request.NewStock);

            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for stock update", request.ProductId);
                return false;
            }

            product.Stock = request.NewStock;
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            
            _logger.LogInformation("Stock updated successfully for product {ProductId}", request.ProductId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", request.ProductId);
            throw;
        }
    }
}
