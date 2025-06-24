using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/auth")]
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
    /// <param name="request">Datos de login del usuario</param>
    /// <returns>Información del usuario y token de acceso</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("User login attempt for: {Email}", request.Email);
            
            var response = await _authService.LoginAsync(request);
            
            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Failed login attempt for: {Email} - {Message}", request.Email, ex.Message);
            return Unauthorized(new { error = "Invalid credentials", message = "The provided email or password is incorrect" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid login request for: {Email} - {Message}", request.Email, ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for: {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Registra un nuevo usuario
    /// </summary>
    /// <param name="request">Datos del nuevo usuario</param>
    /// <returns>Información del usuario registrado y token de acceso</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            _logger.LogInformation("User registration attempt for: {Email}", request.Email);
            
            var response = await _authService.RegisterAsync(request);
            
            _logger.LogInformation("User registered successfully: {Email}", request.Email);
            return CreatedAtAction(nameof(Register), response);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
        {
            _logger.LogWarning("Registration failed - user already exists: {Email}", request.Email);
            return Conflict(new { error = "User already exists", message = "A user with this email already exists" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid registration request for: {Email} - {Message}", request.Email, ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for: {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Renueva el token de acceso usando el refresh token
    /// </summary>
    /// <param name="request">Refresh token</param>
    /// <returns>Nuevo token de acceso</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Refresh token request");
            
            var response = await _authService.RefreshTokenAsync(request);
            
            _logger.LogInformation("Token refreshed successfully");
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Invalid refresh token attempt - {Message}", ex.Message);
            return Unauthorized(new { error = "Invalid refresh token", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid refresh token request - {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while refreshing token" });
        }
    }

    /// <summary>
    /// Cierra la sesión del usuario y revoca el refresh token
    /// </summary>
    /// <param name="request">Refresh token a revocar</param>
    /// <returns>Confirmación del logout</returns>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Logout request");
            
            var response = await _authService.LogoutAsync(request);
            
            _logger.LogInformation("User logged out successfully");
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid logout request - {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Inicia el proceso de recuperación de contraseña
    /// </summary>
    /// <param name="request">Email del usuario</param>
    /// <returns>Confirmación del envío</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Forgot password request for: {Email}", request.Email);
            
            var response = await _authService.ForgotPasswordAsync(request);
            
            _logger.LogInformation("Forgot password email sent to: {Email}", request.Email);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid forgot password request - {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for: {Email}", request.Email);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Restablece la contraseña usando el token de recuperación
    /// </summary>
    /// <param name="request">Token y nueva contraseña</param>
    /// <returns>Confirmación del restablecimiento</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            _logger.LogInformation("Reset password request");
            
            var response = await _authService.ResetPasswordAsync(request);
            
            _logger.LogInformation("Password reset successfully");
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Invalid reset password token - {Message}", ex.Message);
            return Unauthorized(new { error = "Invalid or expired token", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid reset password request - {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while resetting password" });
        }
    }
}
