using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Auth;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene el perfil de un usuario específico
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Perfil del usuario</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserProfile(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only access their own profile unless they are admin
            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to access profile of user {UserId}", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Getting profile for user: {UserId}", id);
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { error = "User not found", message = $"User with ID {id} was not found" });
            }
            
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for ID {UserId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting user profile" });
        }
    }

    /// <summary>
    /// Actualiza el perfil de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Perfil actualizado del usuario</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateUserProfile(int id, [FromBody] UpdateProfileRequest request)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            
            // Users can only update their own profile unless they are admin
            if (currentUserId != id && !User.IsInRole("Admin"))
            {
                _logger.LogWarning("User {CurrentUserId} attempted to update profile of user {UserId}", currentUserId, id);
                return Forbid();
            }

            _logger.LogInformation("Updating profile for user: {UserId}", id);
            
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { error = "User not found", message = $"User with ID {id} was not found" });
            }
            
            _logger.LogInformation("Profile updated successfully for user: {UserId}", id);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Profile update failed: {Message}", ex.Message);
            return BadRequest(new { error = "Invalid request", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for ID {UserId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while updating user profile" });
        }
    }

    /// <summary>
    /// Obtiene la lista de todos los usuarios (solo administradores)
    /// </summary>
    /// <returns>Lista de usuarios</returns>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        try
        {
            _logger.LogInformation("Admin getting all users");
            
            var users = await _userService.GetAllUsersAsync();
            
            _logger.LogInformation("Retrieved {Count} users", users.Count());
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all users");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting users" });
        }
    }

    /// <summary>
    /// Elimina un usuario (solo administradores)
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            _logger.LogInformation("Admin deleting user: {UserId}", id);
            
            var result = await _userService.DeleteUserAsync(id);
            if (!result)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { error = "User not found", message = $"User with ID {id} was not found" });
            }
            
            _logger.LogInformation("User {UserId} deleted successfully", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while deleting user" });
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
