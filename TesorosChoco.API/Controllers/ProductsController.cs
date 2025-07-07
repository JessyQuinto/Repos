using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.Application.Commands.Products;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.API.Common;

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
        catch (TesorosChoco.Domain.Exceptions.BusinessRuleViolationException ex)
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
    }

    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="request">Datos actualizados del producto</param>
    /// <returns>Producto actualizado</returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
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
            return Ok(product);
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
        catch (TesorosChoco.Domain.Exceptions.BusinessRuleViolationException ex)
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
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteProduct(int id)
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
            var query = new SearchProductsQuery(
                SearchTerm: q,
                CategoryId: category,
                MinPrice: minPrice,
                MaxPrice: maxPrice,
                ProducerId: producer,
                Featured: featured,
                Limit: Math.Min(limit, 100), // Limit results
                Offset: Math.Max(offset, 0)
            );

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

    /// <summary>
    /// Obtiene un producto por su slug
    /// </summary>
    /// <param name="slug">Slug del producto</param>
    /// <returns>Producto específico</returns>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
    {
        try
        {
            var query = new GetProductBySlugQuery(slug);
            var product = await _mediator.Send(query);
            
            if (product == null)
            {
                _logger.LogWarning("Product with slug {ProductSlug} not found", slug);
                return NotFound(new { 
                    error = "Product not found", 
                    message = $"Product with slug '{slug}' was not found" 
                });
            }
            
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with slug {ProductSlug}", slug);
            return StatusCode(500, new { 
                error = "Internal server error", 
                message = "An error occurred while getting the product" 
            });
        }
    }

    /// <summary>
    /// Cambia el estado de un producto (solo admins)
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="status">Nuevo estado</param>
    /// <returns>Producto actualizado</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> ChangeProductStatus(int id, [FromBody] ProductStatus status)
    {
        try
        {
            var command = new ChangeProductStatusCommand(id, status);
            var product = await _mediator.Send(command);
            
            if (product == null)
            {
                return NotFound(new { error = "Product not found" });
            }
            
            _logger.LogInformation("Product status changed: {ProductId} -> {Status}", id, status);
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing product status: {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Obtiene todos los productos (incluyendo inactivos) - Solo admins
    /// </summary>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAllProductsForAdmin(
        [FromQuery] ProductStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = new GetAllProductsForAdminQuery
            {
                Status = status,
                Page = page,
                PageSize = Math.Min(pageSize, 100)
            };
            
            var products = await _mediator.Send(query);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products for admin");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Actualiza solo el stock de un producto
    /// </summary>
    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateProductStock(int id, [FromBody] int newStock)
    {
        try
        {
            var command = new UpdateProductStockCommand(id, newStock);
            var success = await _mediator.Send(command);
            
            if (!success)
            {
                return NotFound(new { error = "Product not found" });
            }
            
            return Ok(new { message = "Stock updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product stock: {ProductId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
