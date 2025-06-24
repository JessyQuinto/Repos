using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(int id);
    Task<Cart?> GetByUserIdAsync(int userId);
    Task<Cart> CreateAsync(Cart cart);
    Task<Cart> UpdateAsync(Cart cart);
    Task DeleteAsync(int id);
}
