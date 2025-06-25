using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/cart")]
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
    }    /// <summary>
    /// Obtiene el carrito del usuario autenticado
    /// </summary>
    /// <returns>Carrito del usuario</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCart()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting cart for user: {UserId}", userId);
            
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            
            return Ok(ApiResponse<CartDto>.SuccessResponse(cart, "Cart retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<CartDto>.ErrorResponse("An error occurred while getting the cart"));
        }
    }/// <summary>
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
            
            var cart = await _cartService.AddToCartAsync(userId, request);
            
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
    /// Agrega un item específico al carrito
    /// </summary>
    /// <param name="request">Item a agregar</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPost("add")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> AddToCart([FromBody] AddCartItemRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Adding item to cart for user: {UserId}, ProductId: {ProductId}", userId, request.ProductId);
            
            var cart = await _cartService.AddToCartAsync(userId, request);
            
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
    /// Elimina un item específico del carrito
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
            _logger.LogInformation("Removing item from cart for user: {UserId}, ProductId: {ProductId}", userId, productId);
            
            var cart = await _cartService.RemoveFromCartAsync(userId, productId);
            
            _logger.LogInformation("Item removed from cart successfully for user: {UserId}", userId);
            return Ok(cart);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to remove item from cart: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {            _logger.LogError(ex, "Error removing item from cart");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while removing item from cart" });
        }
    }

    /// <summary>
    /// Reserva stock para todos los items del carrito antes del checkout
    /// </summary>
    /// <returns>Resultado de la reserva</returns>
    [HttpPost("reserve-stock")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ReserveCartStock()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessionId = HttpContext.Session.Id;
            
            _logger.LogInformation("Reserving stock for cart of user: {UserId}", userId);
            
            var success = await _cartService.ReserveCartStockAsync(userId, sessionId);
            
            if (success)
            {
                _logger.LogInformation("Stock reserved successfully for user: {UserId}", userId);
                return Ok(ApiResponse.SuccessResponse("Stock reserved successfully"));
            }
            else
            {
                _logger.LogWarning("Failed to reserve stock for user: {UserId}", userId);
                return BadRequest(ApiResponse.ErrorResponse("Unable to reserve stock. Some items may be out of stock."));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for cart");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse.ErrorResponse("An error occurred while reserving stock"));
        }
    }

    /// <summary>
    /// Libera las reservas de stock del carrito
    /// </summary>
    /// <returns>Resultado de la liberación</returns>
    [HttpPost("release-reservations")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ReleaseCartReservations()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Releasing cart reservations for user: {UserId}", userId);
            
            await _cartService.ReleaseCartReservationsAsync(userId);
            
            _logger.LogInformation("Cart reservations released successfully for user: {UserId}", userId);
            return Ok(ApiResponse.SuccessResponse("Reservations released successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing cart reservations");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse.ErrorResponse("An error occurred while releasing reservations"));
        }
    }

    /// <summary>
    /// Valida la disponibilidad de stock de todos los items del carrito
    /// </summary>
    /// <returns>Resultado de la validación</returns>
    [HttpGet("validate-stock")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ValidateCartStock()
    {
        try
        {
            var userId = GetCurrentUserId();
            
            _logger.LogInformation("Validating cart stock for user: {UserId}", userId);
            
            var isValid = await _cartService.ValidateCartStockAsync(userId);
            
            if (isValid)
            {
                _logger.LogInformation("Cart stock validation passed for user: {UserId}", userId);
                return Ok(ApiResponse.SuccessResponse("All items are available"));
            }
            else
            {
                _logger.LogWarning("Cart stock validation failed for user: {UserId}", userId);
                return Ok(ApiResponse.ErrorResponse("Some items are out of stock"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating cart stock");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse.ErrorResponse("An error occurred while validating stock"));
        }
    }    /// <summary>
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
    }    // Alternative routes without versioning for documentation compatibility
    
    /// <summary>
    /// Obtiene el carrito del usuario autenticado (ruta alternativa sin versionado)
    /// </summary>
    /// <returns>Carrito del usuario</returns>
    [HttpGet("/api/cart")]
    [ProducesResponseType(typeof(ApiResponse<CartDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CartDto>>> GetCartAlternative()
    {
        return await GetCart();
    }

    /// <summary>
    /// Actualiza el carrito del usuario (ruta alternativa sin versionado)
    /// </summary>
    /// <param name="request">Items del carrito a actualizar</param>
    /// <returns>Carrito actualizado</returns>
    [HttpPost("/api/cart")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CartDto>> UpdateCartAlternative([FromBody] UpdateCartRequest request)
    {
        return await UpdateCart(request);
    }
}
