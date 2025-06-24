// This file contains all service interfaces for easier dependency injection registration

using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Auth;
using TesorosChoco.Application.DTOs.Parameters;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;

namespace TesorosChoco.Application.Interfaces;

// Authentication and User Management
public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserDto> GetProfileAsync(int userId);
    Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task LogoutAsync(int userId);
}

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> UpdateUserAsync(int id, UpdateProfileRequest request);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(int id);
}

// Product Management
public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> GetProductBySlugAsync(string slug);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task UpdateProductAsync(UpdateProductRequest request);
    Task DeleteProductAsync(int id);
    Task<PagedResult<ProductDto>> SearchProductsAsync(ProductSearchParameters parameters);
    Task<IEnumerable<ProductDto>> GetFeaturedProductsAsync();
    Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(int categoryId);
    Task<IEnumerable<ProductDto>> GetProductsByProducerAsync(int producerId);
}

public interface ICategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto?> GetCategoryBySlugAsync(string slug);
}

public interface IProducerService
{
    Task<IEnumerable<ProducerDto>> GetAllProducersAsync();
    Task<ProducerDto?> GetProducerByIdAsync(int id);
    Task<IEnumerable<ProducerDto>> GetFeaturedProducersAsync();
}

// Shopping Cart
public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
    Task<CartDto> RemoveFromCartAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
    Task<CartDto> SyncCartAsync(int userId, UpdateCartRequest request);
}

// Order Management
public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(int userId, CreateOrderRequest request);
    Task<OrderDto?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
    Task<OrderDto?> UpdateOrderStatusAsync(int orderId, string status);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
}

// Contact and Newsletter
public interface IContactService
{
    Task<GenericResponse> SubmitContactFormAsync(ContactRequest request);
}

public interface INewsletterService
{
    Task<GenericResponse> SubscribeAsync(NewsletterSubscriptionRequest request);
    Task<GenericResponse> UnsubscribeAsync(string email);
}
