using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Enums;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository, 
        IProductRepository productRepository,
        ICartRepository cartRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cartRepository = cartRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request)
    {
        // Validate items
        if (request.Items == null || !request.Items.Any())
            throw new ArgumentException("Order must contain at least one item");

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>(),
            ShippingAddress = new ShippingAddress
            {
                Name = request.ShippingAddress.Name,
                Address = request.ShippingAddress.Address,
                City = request.ShippingAddress.City,
                ZipCode = request.ShippingAddress.ZipCode,
                Phone = request.ShippingAddress.Phone
            },
            PaymentMethod = request.PaymentMethod,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Add items and calculate total
        decimal total = 0;
        foreach (var itemRequest in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product == null)
                throw new ArgumentException($"Product with ID {itemRequest.ProductId} not found");

            if (product.Stock < itemRequest.Quantity)
                throw new InvalidOperationException($"Insufficient stock for product {product.Name}");

            var orderItem = new OrderItem
            {
                ProductId = itemRequest.ProductId,
                Quantity = itemRequest.Quantity,
                Price = itemRequest.Price
            };

            order.Items.Add(orderItem);
            total += orderItem.Price * orderItem.Quantity;
        }

        order.Total = total;

        // Create the order
        var createdOrder = await _orderRepository.CreateAsync(order);

        // Update product stock
        foreach (var item in createdOrder.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.Stock -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }
        }

        // Clear user's cart
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart != null)
        {
            cart.Items.Clear();
            cart.Total = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);
        }

        return _mapper.Map<OrderDto>(createdOrder);
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        return order != null ? _mapper.Map<OrderDto>(order) : null;
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        var orders = await _orderRepository.GetByUserIdAsync(userId);
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _orderRepository.UpdateAsync(order);
        return _mapper.Map<OrderDto>(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<OrderDto>>(orders);
    }

    public async Task DeleteOrderAsync(int orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null)
            throw new ArgumentException($"Order with ID {orderId} not found");

        await _orderRepository.DeleteAsync(orderId);
    }
}
