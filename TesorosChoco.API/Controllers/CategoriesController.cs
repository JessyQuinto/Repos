using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/categories")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(
        ICategoryService categoryService, 
        IProductService productService,
        ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las categorías de productos
    /// </summary>
    /// <returns>Lista de categorías</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
    {
        try
        {
            _logger.LogInformation("Getting all categories");
            
            var categories = await _categoryService.GetAllCategoriesAsync();
            
            _logger.LogInformation("Retrieved {Count} categories", categories.Count());
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting categories" });
        }
    }

    /// <summary>
    /// Obtiene una categoría específica por su ID
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Categoría específica</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CategoryDto>> GetCategory(int id)
    {
        try
        {
            _logger.LogInformation("Getting category with ID: {CategoryId}", id);
            
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", id);
                return NotFound(new { error = "Category not found", message = $"Category with ID {id} was not found" });
            }
            
            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category with ID {CategoryId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting the category" });
        }
    }

    /// <summary>
    /// Obtiene productos de una categoría específica
    /// </summary>
    /// <param name="categoryId">ID de la categoría</param>
    /// <returns>Lista de productos de la categoría</returns>
    [HttpGet("{categoryId}/products")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(int categoryId)
    {
        try
        {
            _logger.LogInformation("Getting products for category: {CategoryId}", categoryId);
            
            // First verify the category exists
            var category = await _categoryService.GetCategoryByIdAsync(categoryId);
            if (category == null)
            {
                _logger.LogWarning("Category with ID {CategoryId} not found", categoryId);
                return NotFound(new { error = "Category not found", message = $"Category with ID {categoryId} was not found" });
            }
            
            var products = await _productService.GetProductsByCategoryAsync(categoryId);
            
            _logger.LogInformation("Retrieved {Count} products for category {CategoryId}", products.Count(), categoryId);
            return Ok(products);
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for category {CategoryId}", categoryId);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting products for the category" });
        }
    }
}
