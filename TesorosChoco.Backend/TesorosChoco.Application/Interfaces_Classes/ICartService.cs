using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;

namespace TesorosChoco.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartRequest request);
    Task<CartDto> UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
    Task<CartDto> RemoveFromCartAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
    Task<CartDto> SyncCartAsync(int userId, UpdateCartRequest request);
}
