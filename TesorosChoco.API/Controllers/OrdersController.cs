using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva orden de compra
    /// </summary>
    /// <param name="request">Datos de la orden</param>
    /// <returns>Orden creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating order for user: {UserId}", userId);
            
            var order = await _orderService.CreateOrderAsync(request);
            
            _logger.LogInformation("Order {OrderId} created successfully for user {UserId}", order.Id, userId);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to create order: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create order: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid operation", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Obtiene una orden específica por su ID
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Orden específica</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> GetOrder(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting order {OrderId} for user {UserId}", id, userId);
            
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound(new { error = "Order not found", message = $"Order with ID {id} was not found" });
            }

            // Verify the order belongs to the current user (unless admin)
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserId} attempted to access order {OrderId} belonging to user {OrderUserId}", 
                    userId, id, order.UserId);
                return Forbid();
            }
            
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting the order" });
        }
    }

    /// <summary>
    /// Obtiene todas las órdenes del usuario autenticado
    /// </summary>
    /// <returns>Lista de órdenes del usuario</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting orders for user: {UserId}", userId);
            
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orders.Count(), userId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting orders" });
        }
    }

    /// <summary>
    /// Obtiene todas las órdenes de un usuario específico (solo administradores)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Lista de órdenes del usuario</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByUserId(int userId)
    {
        try
        {
            _logger.LogInformation("Admin getting orders for user: {UserId}", userId);
            
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orders.Count(), userId);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting orders" });
        }
    }

    /// <summary>
    /// Actualiza el estado de una orden (solo administradores)
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <param name="request">Nuevo estado</param>
    /// <returns>Orden actualizada</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating status of order {OrderId} to {Status}", id, request.Status);
            
            var order = await _orderService.UpdateOrderStatusAsync(id, request);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound(new { error = "Order not found", message = $"Order with ID {id} was not found" });
            }
            
            _logger.LogInformation("Order {OrderId} status updated successfully to {Status}", id, request.Status);
            return Ok(order);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update order status: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid status", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating order status" });        }
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
