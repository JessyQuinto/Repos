using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public CartService(ICartRepository cartRepository, IProductRepository productRepository, IMapper mapper)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<CartDto> GetCartByUserIdAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            // Create a new empty cart for the user
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>(),
                Total = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            cart = await _cartRepository.CreateAsync(cart);
        }

        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>(),
                Total = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            cart = await _cartRepository.CreateAsync(cart);
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
            throw new ArgumentException($"Product with ID {request.ProductId} not found");

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.Price = product.DiscountedPrice ?? product.Price;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                Price = product.DiscountedPrice ?? product.Price
            });
        }

        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemRequest request)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new ArgumentException("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item == null)
            throw new InvalidOperationException("Cart item not found");

        if (request.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = request.Quantity;
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            item.Price = product?.DiscountedPrice ?? product?.Price ?? item.Price;
        }

        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> RemoveFromCartAsync(int userId, int productId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
            throw new ArgumentException("Cart not found");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException("Cart item not found");

        cart.Items.Remove(item);
        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }

    public async Task ClearCartAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart != null)
        {
            cart.Items.Clear();
            cart.Total = 0;
            cart.UpdatedAt = DateTime.UtcNow;
            await _cartRepository.UpdateAsync(cart);
        }
    }

    public async Task<CartDto> SyncCartAsync(int userId, UpdateCartRequest request)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);
        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                Items = new List<CartItem>(),
                CreatedAt = DateTime.UtcNow
            };
            cart = await _cartRepository.CreateAsync(cart);
        }

        // Clear existing items
        cart.Items.Clear();

        // Add items from request
        foreach (var itemRequest in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(itemRequest.ProductId);
            if (product != null)
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    Price = product.DiscountedPrice ?? product.Price
                });
            }
        }

        cart.Total = cart.Items.Sum(i => i.Price * i.Quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await _cartRepository.UpdateAsync(cart);
        return _mapper.Map<CartDto>(cart);
    }
}
