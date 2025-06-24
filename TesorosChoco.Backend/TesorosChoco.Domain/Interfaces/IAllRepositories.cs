// Repository interfaces that need to be implemented in Infrastructure layer

using TesorosChoco.Domain.Entities;
using TesorosChoco.Application.DTOs.Parameters;

namespace TesorosChoco.Domain.Interfaces;

// User Repository
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

// Product Repository
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<Product?> GetBySlugAsync(string slug);
    Task<IEnumerable<Product>> GetAllAsync();
    Task<IEnumerable<Product>> GetFeaturedAsync();
    Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
    Task<IEnumerable<Product>> GetByProducerAsync(int producerId);
    Task<(IEnumerable<Product> Products, int Total)> SearchAsync(ProductSearchParameters parameters);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
}

// Category Repository
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetBySlugAsync(string slug);
    Task<IEnumerable<Category>> GetAllAsync();
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(int id);
}

// Producer Repository
public interface IProducerRepository
{
    Task<Producer?> GetByIdAsync(int id);
    Task<IEnumerable<Producer>> GetAllAsync();
    Task<IEnumerable<Producer>> GetFeaturedAsync();
    Task AddAsync(Producer producer);
    Task UpdateAsync(Producer producer);
    Task DeleteAsync(int id);
}

// Cart Repository
public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart> AddAsync(Cart cart);
    Task UpdateAsync(Cart cart);
    Task DeleteAsync(int id);
    Task ClearCartAsync(int userId);
}

// Order Repository
public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<IEnumerable<Order>> GetAllAsync();
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(int id);
}

// Contact Message Repository
public interface IContactMessageRepository
{
    Task AddAsync(ContactMessage message);
    Task<IEnumerable<ContactMessage>> GetAllAsync();
}

// Newsletter Subscription Repository
public interface INewsletterSubscriptionRepository
{
    Task<NewsletterSubscription?> GetByEmailAsync(string email);
    Task AddAsync(NewsletterSubscription subscription);
    Task DeleteAsync(string email);
    Task<IEnumerable<NewsletterSubscription>> GetAllAsync();
}

// Domain Services (to be implemented in Infrastructure)
public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}
