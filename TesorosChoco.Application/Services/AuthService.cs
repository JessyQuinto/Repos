using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Authentication service implementation with JWT token management
/// Handles user registration, login, and password security
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            var email = request.Email.Trim().ToLowerInvariant();

            // Get user by email
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate JWT token and refresh token
            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);            // Update last login (if this property exists)
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                User = _mapper.Map<UserDto>(user),
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                RefreshTokenExpiresIn = 604800 // 7 days
            };
        }
        catch (Exception ex) when (!(ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException("Error during login", ex);
        }
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required");

            if (string.IsNullOrWhiteSpace(request.FirstName))
                throw new ArgumentException("First name is required");

            if (string.IsNullOrWhiteSpace(request.LastName))
                throw new ArgumentException("Last name is required");

            var email = request.Email.Trim().ToLowerInvariant();

            // Check if user already exists
            var existingUser = await _userRepository.GetByEmailAsync(email);
            if (existingUser != null)
                throw new InvalidOperationException("A user with this email already exists");

            // Hash password
            var passwordHash = HashPassword(request.Password);

            // Create new user
            var user = new User
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                PasswordHash = passwordHash,
                Phone = request.Phone?.Trim() ?? string.Empty,
                Address = request.Address?.Trim() ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);

            // Generate JWT token and refresh token
            var token = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _refreshTokenService.CreateRefreshTokenAsync(user.Id);            return new AuthResponse
            {
                User = _mapper.Map<UserDto>(user),
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                RefreshTokenExpiresIn = 604800 // 7 days
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException("Error during registration", ex);
        }
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token
    /// </summary>
    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            throw new ArgumentException("Refresh token is required");

        try
        {
            // For this basic implementation, we need the user ID
            // In a more complex system, you might extract this from the old token
            if (!request.UserId.HasValue || request.UserId.Value <= 0)
                throw new ArgumentException("User ID is required for token refresh");

            var userId = request.UserId.Value;

            // Validate the refresh token
            var isValidRefreshToken = await _refreshTokenService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
            if (!isValidRefreshToken)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");

            // Get user data
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            // Revoke the old refresh token
            await _refreshTokenService.RevokeRefreshTokenAsync(userId, request.RefreshToken);

            // Generate new tokens
            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = await _refreshTokenService.CreateRefreshTokenAsync(userId);            return new AuthResponse
            {
                User = _mapper.Map<UserDto>(user),
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = 3600, // 1 hour
                RefreshTokenExpiresIn = 604800 // 7 days
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException("Error refreshing token", ex);
        }
    }

    /// <summary>
    /// Logs out user and revokes refresh token
    /// </summary>
    public async Task<GenericResponse> LogoutAsync(RefreshTokenRequest request)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(request.RefreshToken) && request.UserId.HasValue && request.UserId > 0)
            {
                // Revoke the specific refresh token
                await _refreshTokenService.RevokeRefreshTokenAsync(request.UserId.Value, request.RefreshToken);
            }

            return new GenericResponse
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }
        catch (Exception)
        {
            // Even if token revocation fails, return success for security
            return new GenericResponse
            {
                Success = true,
                Message = "Logged out successfully"
            };
        }
    }

    /// <summary>
    /// Initiates password recovery process
    /// </summary>
    public async Task<GenericResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);

            // Always return success for security reasons (don't reveal if email exists)
            if (user == null)
            {
                return new GenericResponse
                {
                    Success = true,
                    Message = "If the email exists in our system, you will receive password reset instructions"
                };
            }

            // Generate a simple reset token (in production, use a more secure approach)
            var resetToken = GeneratePasswordResetToken(user.Email, user.Id);

            // TODO: Send email with reset link containing the token
            // For now, just log the token (NEVER do this in production!)
            System.Console.WriteLine($"Password reset token for {user.Email}: {resetToken}");

            return new GenericResponse
            {
                Success = true,
                Message = "If the email exists in our system, you will receive password reset instructions"
            };
        }
        catch (Exception)
        {
            // Return generic success message for security
            return new GenericResponse
            {
                Success = true,
                Message = "If the email exists in our system, you will receive password reset instructions"
            };
        }
    }

    /// <summary>
    /// Resets password using recovery token
    /// </summary>
    public async Task<GenericResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            throw new ArgumentException("Reset token is required");

        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ArgumentException("Email is required");

        if (string.IsNullOrWhiteSpace(request.NewPassword))
            throw new ArgumentException("New password is required");

        if (request.NewPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long");

        try
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // For security, don't reveal if user exists
                throw new UnauthorizedAccessException("Invalid reset token or email");
            }

            // Simple token validation: check if token matches a basic pattern
            // In production, use proper token validation with expiration
            var expectedToken = GeneratePasswordResetToken(user.Email, user.Id);
            if (request.Token != expectedToken)
            {
                throw new UnauthorizedAccessException("Invalid or expired reset token");
            }

            // Update password
            user.PasswordHash = HashPassword(request.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            
            await _userRepository.UpdateAsync(user);

            // Revoke all refresh tokens for this user (security measure)
            await _refreshTokenService.RevokeAllRefreshTokensAsync(user.Id);

            return new GenericResponse
            {
                Success = true,
                Message = "Password has been reset successfully"
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is UnauthorizedAccessException))
        {
            throw new InvalidOperationException("Error resetting password", ex);
        }
    }

    /// <summary>
    /// Generate a simple password reset token
    /// In production, use a more secure approach with expiration times
    /// </summary>
    private static string GeneratePasswordResetToken(string email, int userId)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var data = $"{email}|{userId}|{timestamp}|TesorosChoco_Reset_Salt";
        
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Hash password using a secure hashing algorithm
    /// In production, use BCrypt or Argon2
    /// </summary>
    private static string HashPassword(string password)
    {
        // TODO: Implement proper password hashing (BCrypt, Argon2, etc.)
        // This is a simplified implementation for development - DO NOT USE IN PRODUCTION
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "TesorosChoco_Salt"));
        return Convert.ToBase64String(hashedBytes);
    }

    /// <summary>
    /// Verify password against stored hash
    /// </summary>
    private static bool VerifyPassword(string password, string hash)
    {
        // TODO: Implement proper password verification
        // This is a simplified implementation for development - DO NOT USE IN PRODUCTION
        try
        {
            var passwordHash = HashPassword(password);
            return passwordHash == hash;
        }
        catch
        {
            return false;
        }
    }
}
