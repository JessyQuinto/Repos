using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;

namespace TesorosChoco.Application.Interfaces;

public interface ICartService
{
    Task<CartDto> GetCartByUserIdAsync(int userId);
    Task<CartDto> SyncCartAsync(int userId, UpdateCartRequest request);
    Task<CartDto> AddToCartAsync(int userId, AddCartItemRequest request);
    Task<CartDto> RemoveFromCartAsync(int userId, int productId);
    Task<CartDto> UpdateItemAsync(int userId, int itemId, UpdateCartItemRequest request);
    Task<CartDto> RemoveItemAsync(int userId, int itemId);
    Task ClearCartAsync(int userId);
}
