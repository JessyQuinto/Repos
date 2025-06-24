using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
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
}
