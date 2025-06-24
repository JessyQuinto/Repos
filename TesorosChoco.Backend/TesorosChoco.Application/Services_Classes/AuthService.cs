using Microsoft.Extensions.Logging;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Auth;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Domain.Entities;
using AutoMapper;

namespace TesorosChoco.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordService _passwordService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordService passwordService,
        IMapper mapper,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordService = passwordService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", request.Email);

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed: User not found with email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Verify password
            if (!_passwordService.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user's refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("User {Email} logged in successfully", request.Email);

            return new AuthResponse
            {
                User = userDto,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 3600, // 1 hour in seconds
                RefreshTokenExpiresIn = 2592000 // 30 days in seconds
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            throw new Exception("An error occurred during login");
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", request.Email);

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed: User already exists with email {Email}", request.Email);
                throw new InvalidOperationException("User with this email already exists");
            }

            // Create new user
            var passwordHash = _passwordService.HashPassword(request.Password);
            
            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user's refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("User {Email} registered successfully", request.Email);

            return new AuthResponse
            {
                User = userDto,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshTokenExpiresIn = 2592000
            };
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
            throw new Exception("An error occurred during registration");
        }
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting token refresh");

            // Validate refresh token
            var user = await _userRepository.GetByRefreshTokenAsync(request.RefreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                _logger.LogWarning("Token refresh failed: Invalid or expired refresh token");
                throw new UnauthorizedAccessException("Invalid or expired refresh token");
            }

            // Generate new tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Update user's refresh token
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Token refreshed successfully for user {UserId}", user.Id);

            return new AuthResponse
            {
                User = userDto,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                RefreshTokenExpiresIn = 2592000
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            throw new Exception("An error occurred during token refresh");
        }
    }

    public async Task<UserDto> GetProfileAsync(int userId)
    {
        try
        {
            _logger.LogInformation("Getting profile for user: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Profile not found for user {UserId}", userId);
                throw new ArgumentException($"User with ID {userId} not found");
            }

            return _mapper.Map<UserDto>(user);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting profile for user {UserId}", userId);
            throw new Exception("An error occurred while getting user profile");
        }
    }

    public async Task<UserDto> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            _logger.LogInformation("Updating profile for user: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Profile update failed: User {UserId} not found", userId);
                throw new ArgumentException($"User with ID {userId} not found");
            }

            // Update user properties
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.Address = request.Address;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);

            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            return userDto;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
            throw new Exception("An error occurred while updating user profile");
        }
    }

    public async Task LogoutAsync(int userId)
    {
        try
        {
            _logger.LogInformation("User {UserId} logging out", userId);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                // Invalidate refresh token
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);
            }

            _logger.LogInformation("User {UserId} logged out successfully", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", userId);
            throw new Exception("An error occurred during logout");
        }
    }
}
