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

    public async Task<CartDto> AddItemAsync(int userId, AddCartItemRequest request)
    {
        try
        {
            if (request.ProductId <= 0)
                throw new ArgumentException("Product ID must be positive");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be positive");

            // Validate product exists
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            if (product == null)
                throw new ArgumentException($"Product with ID {request.ProductId} does not exist");

            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
            {
                // Create new cart if it doesn't exist
                cart = new Cart
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Items = new List<CartItem>()
                };
                cart = await _cartRepository.AddAsync(cart);
            }

            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
            if (existingItem != null)
            {
                // Update quantity
                existingItem.Quantity += request.Quantity;
                existingItem.Price = product.CurrentPrice; // Update to current price
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Price = product.CurrentPrice,
                    CartId = cart.Id
                };
                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return _mapper.Map<CartDto>(cart);
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            throw new InvalidOperationException($"Error adding item to cart for user {userId}", ex);
        }
    }

    public async Task<CartDto> UpdateItemAsync(int userId, int itemId, UpdateCartItemRequest request)
    {
        try
        {
            if (request.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");

            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
                throw new KeyNotFoundException("Cart not found for user");

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            if (request.Quantity == 0)
            {
                // Remove item if quantity is 0
                cart.Items.Remove(cartItem);
            }
            else
            {
                // Update quantity
                cartItem.Quantity = request.Quantity;
                
                // Update price to current price
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    cartItem.Price = product.CurrentPrice;
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return _mapper.Map<CartDto>(cart);
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is KeyNotFoundException))
        {
            throw new InvalidOperationException($"Error updating cart item {itemId} for user {userId}", ex);
        }
    }

    public async Task<CartDto> RemoveItemAsync(int userId, int itemId)
    {
        try
        {
            var cart = await _cartRepository.GetByUserIdAsync(userId);
            if (cart == null)
                throw new KeyNotFoundException("Cart not found for user");

            var cartItem = cart.Items.FirstOrDefault(i => i.Id == itemId);
            if (cartItem == null)
                throw new KeyNotFoundException("Cart item not found");

            cart.Items.Remove(cartItem);
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);

            return _mapper.Map<CartDto>(cart);
        }
        catch (Exception ex) when (!(ex is KeyNotFoundException))
        {
            throw new InvalidOperationException($"Error removing cart item {itemId} for user {userId}", ex);
        }
    }
}
