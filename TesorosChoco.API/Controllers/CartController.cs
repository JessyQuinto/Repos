using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(ICartService cartService, ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el carrito del usuario autenticado
    /// </summary>
    /// <returns>Carrito del usuario</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> GetCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting cart for user: {UserId}", userId);
            
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            
            return Ok(cart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting the cart" });
        }
    }    /// <summary>
    /// Actualiza el carrito del usuario (agregar/modificar items)
    /// </summary>
    /// <param name="request">Datos del carrito a actualizar</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> UpdateCart([FromBody] UpdateCartRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating cart for user: {UserId}", userId);
            
            var cart = await _cartService.SyncCartAsync(userId, request);
            
            _logger.LogInformation("Cart updated successfully for user: {UserId}", userId);
            return Ok(cart);
        }        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update cart: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating cart" });
        }
    }

    /// <summary>
    /// Vacía completamente el carrito del usuario
    /// </summary>
    /// <returns>No content</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Clearing cart for user: {UserId}", userId);
            
            await _cartService.ClearCartAsync(userId);
            
            _logger.LogInformation("Cart cleared successfully for user: {UserId}", userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while clearing the cart" });
        }
    }

    /// <summary>
    /// Agrega un item específico al carrito
    /// </summary>
    /// <param name="request">Datos del item a agregar</param>
    /// <returns>Item agregado</returns>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> AddCartItem([FromBody] AddCartItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Adding item to cart for user: {UserId}, Product: {ProductId}", userId, request.ProductId);
            
            var cart = await _cartService.AddItemAsync(userId, request);
            
            _logger.LogInformation("Item added to cart successfully for user: {UserId}", userId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to add item to cart: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while adding item to cart" });
        }
    }

    /// <summary>
    /// Actualiza la cantidad de un item específico del carrito
    /// </summary>
    /// <param name="id">ID del item del carrito</param>
    /// <param name="request">Nueva cantidad</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPut("items/{id}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> UpdateCartItem(int id, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating cart item {ItemId} for user: {UserId}", id, userId);
            
            var cart = await _cartService.UpdateItemAsync(userId, id, request);
            
            _logger.LogInformation("Cart item updated successfully for user: {UserId}", userId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update cart item: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cart item not found: {Message}", ex.Message);
            return NotFound(new { error = "Item not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating cart item" });
        }
    }

    /// <summary>
    /// Remueve un item específico del carrito
    /// </summary>
    /// <param name="id">ID del item del carrito</param>
    /// <returns>Carrito actualizado</returns>
    [HttpDelete("items/{id}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> RemoveCartItem(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Removing cart item {ItemId} for user: {UserId}", id, userId);
            
            var cart = await _cartService.RemoveItemAsync(userId, id);
            
            _logger.LogInformation("Cart item removed successfully for user: {UserId}", userId);
            return Ok(cart);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Cart item not found: {Message}", ex.Message);
            return NotFound(new { error = "Item not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cart item");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while removing cart item" });
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde el token JWT
    /// </summary>
    /// <returns>ID del usuario</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}
