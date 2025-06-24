using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Auth;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Autentica a un usuario existente
    /// </summary>
    /// <param name="request">Credenciales de login</param>
    /// <returns>Información del usuario y token de acceso</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", request.Email);
            
            var result = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for user {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { error = "Invalid credentials", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    /// <param name="request">Datos de registro</param>
    /// <returns>Información del usuario creado y token de acceso</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting registration for user: {Email}", request.Email);
            
            var result = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User {Email} registered successfully", request.Email);
            return CreatedAtAction(nameof(GetProfile), new { id = result.User.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Registration failed for user {Email}: {Message}", request.Email, ex.Message);
            return Conflict(new { error = "User already exists", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Renueva el token de acceso usando el refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Nuevo token de acceso</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Attempting token refresh");
            
            var result = await _authService.RefreshTokenAsync(request);
            
            _logger.LogInformation("Token refreshed successfully");
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { error = "Invalid refresh token", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Obtiene el perfil del usuario autenticado
    /// </summary>
    /// <returns>Información del perfil del usuario</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting profile for user: {UserId}", userId);
            
            var profile = await _authService.GetProfileAsync(userId);
            
            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Profile not found: {Message}", ex.Message);
            return NotFound(new { error = "User not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting profile" });
        }
    }

    /// <summary>
    /// Actualiza el perfil del usuario autenticado
    /// </summary>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Perfil actualizado del usuario</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Updating profile for user: {UserId}", userId);
            
            var result = await _authService.UpdateProfileAsync(userId, request);
            
            _logger.LogInformation("Profile updated successfully for user: {UserId}", userId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Profile update failed: {Message}", ex.Message);
            return NotFound(new { error = "User not found", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating profile" });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario
    /// </summary>
    /// <returns>Confirmación de logout</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} logging out", userId);
            
            await _authService.LogoutAsync(userId);
            
            _logger.LogInformation("User {UserId} logged out successfully", userId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Obtiene el ID del usuario actual desde el token JWT
    /// </summary>
    /// <returns>ID del usuario</returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst("sub");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }
}
