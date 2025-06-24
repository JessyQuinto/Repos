using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.API.Common;

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
    }    /// <summary>
    /// Crea una nueva orden de compra
    /// </summary>
    /// <param name="request">Datos de la orden</param>
    /// <returns>Orden creada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Creating order for user: {UserId}", userId);
            
            var order = await _orderService.CreateOrderAsync(request);
            
            _logger.LogInformation("Order {OrderId} created successfully for user {UserId}", order.Id, userId);
            return StatusCode(StatusCodes.Status201Created, 
                ApiResponse<OrderDto>.SuccessResponse(order, "Order created successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to create order: {Message}", ex.Message);
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Failed to create order: {Message}", ex.Message);
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<OrderDto>.ErrorResponse("An error occurred while creating the order"));
        }
    }    /// <summary>
    /// Obtiene una orden específica por su ID
    /// </summary>
    /// <param name="id">ID de la orden</param>
    /// <returns>Orden específica</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting order {OrderId} for user {UserId}", id, userId);
            
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound(ApiResponse<OrderDto>.ErrorResponse($"Order with ID {id} was not found"));
            }

            // Verify the order belongs to the current user (unless admin)
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("User {UserId} attempted to access order {OrderId} belonging to user {OrderUserId}", 
                    userId, id, order.UserId);
                return Forbid();
            }
            
            return Ok(ApiResponse<OrderDto>.SuccessResponse(order, "Order retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<OrderDto>.ErrorResponse("An error occurred while getting the order"));
        }
    }    /// <summary>
    /// Obtiene todas las órdenes del usuario autenticado
    /// </summary>
    /// <returns>Lista de órdenes del usuario</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetUserOrders()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting orders for user: {UserId}", userId);
            
            var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orders.Count(), userId);
            return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("An error occurred while getting orders"));
        }
    }

    /// <summary>
    /// Obtiene todas las órdenes de un usuario específico (solo administradores)
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <returns>Lista de órdenes del usuario</returns>
    [HttpGet("user/{userId}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetOrdersByUserId(int userId)
    {
        try
        {
            _logger.LogInformation("Admin getting orders for user: {UserId}", userId);
              var orders = await _orderService.GetOrdersByUserIdAsync(userId);
            
            _logger.LogInformation("Retrieved {Count} orders for user {UserId}", orders.Count(), userId);
            return Ok(ApiResponse<IEnumerable<OrderDto>>.SuccessResponse(orders, "Orders retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<IEnumerable<OrderDto>>.ErrorResponse("An error occurred while getting orders"));
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
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating status of order {OrderId} to {Status}", id, request.Status);
            
            var order = await _orderService.UpdateOrderStatusAsync(id, request);
            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found", id);
                return NotFound(ApiResponse<OrderDto>.ErrorResponse($"Order with ID {id} was not found"));
            }
            
            _logger.LogInformation("Order {OrderId} status updated successfully to {Status}", id, request.Status);
            return Ok(ApiResponse<OrderDto>.SuccessResponse(order, "Order status updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Failed to update order status: {Message}", ex.Message);
            return BadRequest(ApiResponse<OrderDto>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                ApiResponse<OrderDto>.ErrorResponse("An error occurred while updating order status"));
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
