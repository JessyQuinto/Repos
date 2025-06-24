using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.Application.Commands.Products;
using TesorosChoco.Application.DTOs.Requests;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene la lista de productos con filtros opcionales
    /// </summary>
    /// <param name="featured">Filtro por productos destacados</param>
    /// <param name="categoryId">Filtro por categoría</param>
    /// <param name="producerId">Filtro por productor</param>
    /// <param name="searchTerm">Término de búsqueda</param>
    /// <param name="page">Página (por defecto 1)</param>
    /// <param name="pageSize">Tamaño de página (por defecto 10, máximo 100)</param>
    /// <returns>Lista de productos</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
        [FromQuery] bool? featured = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? producerId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetAllProductsQuery
            {
                Featured = featured,
                CategoryId = categoryId,
                ProducerId = producerId,
                SearchTerm = searchTerm,
                Page = page,
                PageSize = Math.Min(pageSize, 100) // Limit page size
            };

            var products = await _mediator.Send(query);
            return Ok(products);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for GetProducts: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { 
                error = "Validation failed", 
                errors = ex.Errors.Select(e => e.ErrorMessage) 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while getting products" 
            });
        }
    }

    /// <summary>
    /// Obtiene un producto específico por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Producto específico</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        try
        {
            var query = new GetProductByIdQuery(id);
            var product = await _mediator.Send(query);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found", id);
                return NotFound(new { 
                    error = "Product not found", 
                    message = $"Product with ID {id} was not found" 
                });
            }
            
            return Ok(product);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for GetProduct: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { 
                error = "Validation failed", 
                errors = ex.Errors.Select(e => e.ErrorMessage) 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while getting the product" 
            });
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
            var command = new CreateProductCommand(request);
            var product = await _mediator.Send(command);
            
            _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for CreateProduct: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { 
                error = "Validation failed", 
                errors = ex.Errors.Select(e => e.ErrorMessage) 
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business rule violation in CreateProduct: {Message}", ex.Message);
            return BadRequest(new { error = "Business rule violation", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product: {ProductName}", request.Name);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while creating the product" 
            });
        }
    }    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto</param>
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
            var command = new UpdateProductCommand(id, request);
            var product = await _mediator.Send(command);
            
            if (product == null)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for update", id);
                return NotFound(new { 
                    error = "Product not found", 
                    message = $"Product with ID {id} was not found" 
                });
            }
            
            _logger.LogInformation("Product updated successfully: {ProductId}", id);
            return NoContent();
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for UpdateProduct: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { 
                error = "Validation failed", 
                errors = ex.Errors.Select(e => e.ErrorMessage) 
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Business rule violation in UpdateProduct: {Message}", ex.Message);
            return BadRequest(new { error = "Business rule violation", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while updating the product" 
            });
        }
    }

    /// <summary>
    /// Elimina un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
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
            var command = new DeleteProductCommand(id);
            var success = await _mediator.Send(command);
            
            if (!success)
            {
                _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                return NotFound(new { 
                    error = "Product not found", 
                    message = $"Product with ID {id} was not found" 
                });
            }
            
            _logger.LogInformation("Product deleted successfully: {ProductId}", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while deleting the product" 
            });
        }
    }

    /// <summary>
    /// Obtiene productos destacados
    /// </summary>
    /// <param name="count">Número de productos a obtener (por defecto 10)</param>
    /// <returns>Lista de productos destacados</returns>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetFeaturedProducts([FromQuery] int count = 10)
    {
        try
        {
            var query = new GetFeaturedProductsQuery(Math.Min(count, 50)); // Limit count
            var products = await _mediator.Send(query);
            
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting featured products");            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while getting featured products" 
            });
        }
    }

    /// <summary>
    /// Busca productos con filtros específicos
    /// </summary>
    /// <param name="q">Término de búsqueda</param>
    /// <param name="category">ID de categoría</param>
    /// <param name="minPrice">Precio mínimo</param>
    /// <param name="maxPrice">Precio máximo</param>
    /// <param name="producer">ID del productor</param>
    /// <param name="featured">Filtro por productos destacados</param>
    /// <param name="limit">Número máximo de resultados</param>
    /// <param name="offset">Número de resultados a omitir</param>
    /// <returns>Resultados de búsqueda paginados</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SearchProductsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchProductsResponse>> SearchProducts(
        [FromQuery] string? q = null,
        [FromQuery] int? category = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int? producer = null,
        [FromQuery] bool? featured = null,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0)
    {
        try
        {
            // Validate parameters
            if (limit > 100) limit = 100; // Max limit
            if (limit < 1) limit = 10;    // Default limit
            if (offset < 0) offset = 0;   // Non-negative offset

            var query = new SearchProductsQuery(
                SearchTerm: q,
                CategoryId: category,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                ProducerId: producer,
                Featured: featured,
                Limit: limit,
                Offset: offset);

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation failed for SearchProducts: {Errors}", 
                string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { 
                error = "Validation failed", 
                errors = ex.Errors.Select(e => e.ErrorMessage) 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while searching products" 
            });
        }
    }
}
