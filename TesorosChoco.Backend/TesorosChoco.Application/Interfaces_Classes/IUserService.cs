using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Auth;

namespace TesorosChoco.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> UpdateUserAsync(int id, UpdateProfileRequest request);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> DeleteUserAsync(int id);
}
