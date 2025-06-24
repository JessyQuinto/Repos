using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// User service implementation with profile management
/// Handles user profile retrieval and updates
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving user with ID {id}", ex);
        }
    }

    public async Task<UserDto> UpdateUserAsync(int id, UpdateProfileRequest request)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new ArgumentException($"User with ID {id} does not exist");            // Update user properties
            _mapper.Map(request, user);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserDto>(user);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating user with ID {id}", ex);
        }
    }
}
