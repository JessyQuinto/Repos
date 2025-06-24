using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Parameters;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene la lista completa de productos
    /// </summary>
    /// <returns>Lista de productos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
    {
        try
        {
            _logger.LogInformation("Getting all products");
            
            var products = await _productService.GetAllProductsAsync();
            
            _logger.LogInformation("Retrieved {Count} products", products.Count());
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting products" });
        }
    }

    /// <summary>
    /// Obtiene un producto específico por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Producto específico</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            _logger.LogInformation("Getting product with ID: {ProductId}", id);
            
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound(new { error = "Product not found", message = $"Product with ID {id} was not found" });
            }
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting the product" });
        }
    }

    /// <summary>
    /// Crea un nuevo producto
    /// </summary>
    /// <param name="request">Datos del producto a crear</param>
    /// <returns>Producto creado</returns>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new product: {ProductName}", request.Name);
            
            var product = await _productService.CreateProductAsync(request);
            
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Product creation failed: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid operation", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while creating the product" });
        }
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="request">Datos actualizados del producto</param>
    /// <returns>No content</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest(new { error = "ID mismatch", message = "ID in URL does not match ID in request body" });
            }            _logger.LogInformation("Updating product with ID: {ProductId}", id);
            
            await _productService.UpdateProductAsync(id, request);
            
            _logger.LogInformation("Product with ID {ProductId} updated successfully", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Product update failed: {Message}", ex.Message);
            return NotFound(new { error = "Product not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating the product" });
        }
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>No content</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        try
        {
            _logger.LogInformation("Deleting product with ID: {ProductId}", id);
            
            await _productService.DeleteProductAsync(id);
            
            _logger.LogInformation("Product with ID {ProductId} deleted successfully", id);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Product deletion failed: {Message}", ex.Message);
            return NotFound(new { error = "Product not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while deleting the product" });
        }
    }

    /// <summary>
    /// Busca productos con filtros específicos
    /// </summary>
    /// <param name="parameters">Parámetros de búsqueda y filtrado</param>
    /// <returns>Productos filtrados con paginación</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PagedResult<ProductDto>>> SearchProducts([FromQuery] ProductSearchParameters parameters)
    {
        try
        {
            _logger.LogInformation("Searching products with parameters: {@Parameters}", parameters);
            
            var result = await _productService.SearchProductsAsync(parameters);
            
            _logger.LogInformation("Found {Count} products matching search criteria", result.Total);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while searching products" });
        }
    }

    /// <summary>
    /// Obtiene productos destacados
    /// </summary>
    /// <returns>Lista de productos destacados</returns>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts()
    {
        try
        {
            _logger.LogInformation("Getting featured products");
            
            var products = await _productService.GetFeaturedProductsAsync();
            
            _logger.LogInformation("Retrieved {Count} featured products", products.Count());
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured products");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting featured products" });
        }
    }}
