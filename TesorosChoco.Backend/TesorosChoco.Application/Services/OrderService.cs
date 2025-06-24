using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Enums;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Order service implementation with order lifecycle management
/// Handles order creation, status updates, and order retrieval
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IUserRepository userRepository,
        IMapper mapper)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        try
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new ArgumentException($"User with ID {request.UserId} does not exist");

            // Validate products and calculate total
            var orderItems = new List<OrderItem>();
            decimal calculatedTotal = 0;

            foreach (var itemRequest in request.Items)
            {
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {itemRequest.ProductId} does not exist");

                if (itemRequest.Quantity <= 0)
                    throw new ArgumentException($"Invalid quantity {itemRequest.Quantity} for product {itemRequest.ProductId}");

                // Check stock availability
                if (product.Stock < itemRequest.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for product {product.Name}. Available: {product.Stock}, Requested: {itemRequest.Quantity}");

                var orderItem = new OrderItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    Price = product.CurrentPrice // Use current price from product, not request
                };

                orderItems.Add(orderItem);
                calculatedTotal += orderItem.Price * orderItem.Quantity;
            }

            // Create shipping address value object
            var shippingAddress = new ShippingAddress
            {
                Name = request.ShippingAddress.Name,
                Address = request.ShippingAddress.Address,
                City = request.ShippingAddress.City,
                ZipCode = request.ShippingAddress.ZipCode,
                Phone = request.ShippingAddress.Phone
            };

            // Create order
            var order = new Order
            {
                UserId = request.UserId,
                Items = orderItems,
                ShippingAddress = shippingAddress,
                PaymentMethod = request.PaymentMethod,
                Total = calculatedTotal, // Use calculated total, not request total
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdOrder = await _orderRepository.CreateAsync(order);
            
            // Reload with navigation properties
            var orderWithDetails = await _orderRepository.GetByIdAsync(createdOrder.Id);
            return _mapper.Map<OrderDto>(orderWithDetails);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error creating order", ex);
        }
    }

    public async Task<OrderDto?> GetOrderByIdAsync(int id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order != null ? _mapper.Map<OrderDto>(order) : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving order with ID {id}", ex);
        }
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
    {
        try
        {
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving orders for user {userId}", ex);
        }
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new ArgumentException($"Order with ID {id} does not exist");            // Validate status
            if (!Enum.TryParse<OrderStatus>(request.Status, true, out var orderStatus))
                throw new ArgumentException($"Invalid status '{request.Status}'. Valid statuses are: {string.Join(", ", Enum.GetNames<OrderStatus>())}");

            order.Status = orderStatus;
            order.UpdatedAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order);
            
            // Reload with navigation properties
            var orderWithDetails = await _orderRepository.GetByIdAsync(updatedOrder.Id);
            return _mapper.Map<OrderDto>(orderWithDetails);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating order status for order {id}", ex);
        }
    }
}
