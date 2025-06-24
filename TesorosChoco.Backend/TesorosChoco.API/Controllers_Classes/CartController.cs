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
