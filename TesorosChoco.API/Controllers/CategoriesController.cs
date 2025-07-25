using Microsoft.AspNetCore.Mvc;
using MediatR;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Produces("application/json")]
public class CategoriesController : BaseController
{
    private readonly ICategoryService _categoryService;
    private readonly IMediator _mediator;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService,
        IMediator mediator,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    /// <summary>
    /// Obtiene todas las categorías de productos
    /// </summary>
    /// <returns>Lista de categorías</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetCategories()
    {
        try
        {
            _logger.LogInformation("Getting all categories");
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            _logger.LogInformation("Retrieved {Count} categories", categories.Count());
            return Ok(ApiResponse<IEnumerable<CategoryDto>>.SuccessResponse(categories, "Categories retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting categories"));
        }
    }    /// <summary>
    /// Obtiene una categoría específica por su ID
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Categoría específica</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return NotFound(ApiResponse.ErrorResponse($"Category with ID {id} was not found"));
            }
            
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Category retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category with ID {CategoryId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting the category"));
        }
    }    /// <summary>
    /// Obtiene productos de una categoría específica
    /// </summary>
    /// <param name="categoryId">ID de la categoría</param>
    /// <returns>Lista de productos de la categoría</returns>
    [HttpGet("{categoryId}/products")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByCategory(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting products for category: {CategoryId}", categoryId);
            
            // First verify the category exists
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", categoryId);
                return NotFound(ApiResponse.ErrorResponse($"Category with ID {categoryId} was not found"));
            }
            
            var query = new GetAllProductsQuery { CategoryId = categoryId };
            var products = await _mediator.Send(query);
            
            _logger.LogInformation("Retrieved {Count} products for category {CategoryId}", products.Count(), categoryId);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for category {CategoryId}", categoryId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting products for the category"));
        }
    }
}
