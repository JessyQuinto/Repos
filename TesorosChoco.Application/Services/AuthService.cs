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
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IMapper mapper)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
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

            // Find user by email
            var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
            if (user == null)
                throw new UnauthorizedAccessException("Invalid email or password");

            // Verify password (assuming password is hashed in the database)
            if (!VerifyPassword(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password");

            // Generate JWT token
            var token = _tokenService.GenerateAccessToken(user);

            // Update last login (if this property exists)
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new AuthResponse
            {
                User = _mapper.Map<UserDto>(user),
                Token = token
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

            // Generate JWT token
            var token = _tokenService.GenerateAccessToken(user);

            return new AuthResponse
            {
                User = _mapper.Map<UserDto>(user),
                Token = token
            };
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            throw new InvalidOperationException("Error during registration", ex);
        }
    }    /// <summary>
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
