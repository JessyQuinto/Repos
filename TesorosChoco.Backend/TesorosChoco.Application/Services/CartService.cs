using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Cart service implementation with comprehensive cart management
/// Handles cart synchronization, item updates, and cart clearing
/// </summary>
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IMapper mapper)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CartDto> GetCartByUserIdAsync(int userId)
    {
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            
            if (cart == null)
            {
                // Create a new cart if none exists
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                cart = await _cartRepository.CreateAsync(cart);
            }

            return _mapper.Map<CartDto>(cart);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving cart for user {userId}", ex);
        }
    }

    public async Task<CartDto> SyncCartAsync(int userId, UpdateCartRequest request)
    {
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
            }

            // Clear existing items
            cart.Items.Clear();

            // Add new items from request
            foreach (var itemRequest in request.Items)
            {
                // Validate product exists
                var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
                if (product == null)
                {
                    throw new ArgumentException($"Product with ID {itemRequest.ProductId} does not exist");
                }

                // Validate quantity is positive
                if (itemRequest.Quantity <= 0)
                {
                    continue; // Skip items with zero or negative quantity
                }

                var cartItem = new CartItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    Price = product.CurrentPrice // Use current price from product
                };

                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            // Save or update cart
            if (cart.Id == 0)
            {
                cart = await _cartRepository.CreateAsync(cart);
            }
            else
            {
                cart = await _cartRepository.UpdateAsync(cart);
            }

            return _mapper.Map<CartDto>(cart);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error syncing cart for user {userId}", ex);
        }
    }

    public async Task ClearCartAsync(int userId)
    {
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            
            if (cart != null)
            {
                cart.Items.Clear();
                cart.UpdatedAt = DateTime.UtcNow;
                await _cartRepository.UpdateAsync(cart);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error clearing cart for user {userId}", ex);
        }
    }
}
