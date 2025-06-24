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
    }

    /// <summary>
    /// Agrega un producto al carrito
    /// </summary>
    /// <param name="request">Información del producto a agregar</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Adding product {ProductId} to cart for user {UserId}", request.ProductId, userId);
            
            var cart = await _cartService.AddToCartAsync(userId, request);
            
            _logger.LogInformation("Product {ProductId} added to cart successfully", request.ProductId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to add to cart: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to add to cart: {Message}", ex.Message);
            return NotFound(new { error = "Product not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding to cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while adding to cart" });
        }
    }

    /// <summary>
    /// Actualiza la cantidad de un producto en el carrito
    /// </summary>
    /// <param name="request">Información de actualización</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPut("update")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> UpdateCartItem([FromBody] UpdateCartItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating cart item {ProductId} for user {UserId}", request.ProductId, userId);
            
            var cart = await _cartService.UpdateCartItemAsync(userId, request);
            
            _logger.LogInformation("Cart item {ProductId} updated successfully", request.ProductId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update cart item: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to update cart item: {Message}", ex.Message);
            return NotFound(new { error = "Cart item not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating cart item" });
        }
    }

    /// <summary>
    /// Elimina un producto del carrito
    /// </summary>
    /// <param name="productId">ID del producto a eliminar</param>
    /// <returns>Carrito actualizado</returns>
    [HttpDelete("remove/{productId}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> RemoveFromCart(int productId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Removing product {ProductId} from cart for user {UserId}", productId, userId);
            
            var cart = await _cartService.RemoveFromCartAsync(userId, productId);
            
            _logger.LogInformation("Product {ProductId} removed from cart successfully", productId);
            return Ok(cart);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to remove from cart: {Message}", ex.Message);
            return NotFound(new { error = "Cart item not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while removing from cart" });
        }
    }

    /// <summary>
    /// Vacía completamente el carrito del usuario
    /// </summary>
    /// <returns>Confirmación de vaciado</returns>
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
    /// Sincroniza el carrito local con el carrito del servidor
    /// </summary>
    /// <param name="request">Datos del carrito a sincronizar</param>
    /// <returns>Carrito sincronizado</returns>
    [HttpPost("sync")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> SyncCart([FromBody] UpdateCartRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Syncing cart for user: {UserId}", userId);
            
            var cart = await _cartService.SyncCartAsync(userId, request);
            
            _logger.LogInformation("Cart synced successfully for user: {UserId}", userId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to sync cart: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while syncing the cart" });
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
