using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Enums;

namespace TesorosChoco.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
}
