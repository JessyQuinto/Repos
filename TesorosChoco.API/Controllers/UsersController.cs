using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/users")]
[Produces("application/json")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }    /// <summary>
    /// Obtiene el perfil de un usuario específico
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Perfil del usuario</returns>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserProfile(int id)    {
        try
        {
            // Users can only access their own profile unless they are admin
            if (!CanAccessUserResource(id))
            {
                _logger.LogWarning("User attempted to access profile of user {UserId} without permission", id);
                return Forbid();
            }

            _logger.LogInformation("Getting profile for user: {UserId}", id);
            
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(ApiResponse.ErrorResponse($"User with ID {id} was not found"));
            }
            
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User profile retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for ID {UserId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting user profile"));
        }
    }    /// <summary>
    /// Actualiza el perfil de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Perfil actualizado del usuario</returns>
    [HttpPut("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUserProfile(int id, [FromBody] UpdateProfileRequest request)    {
        try
        {
            // Users can only update their own profile unless they are admin
            if (!CanAccessUserResource(id))
            {
                _logger.LogWarning("User attempted to update profile of user {UserId} without permission", id);
                return Forbid();
            }

            _logger.LogInformation("Updating profile for user: {UserId}", id);
            
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(ApiResponse.ErrorResponse($"User with ID {id} was not found"));
            }
            
            _logger.LogInformation("Profile updated successfully for user: {UserId}", id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "User profile updated successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Profile update failed: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for ID {UserId}", id);            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating user profile"));
        }
    }

    // Rutas de compatibilidad con documentación API
    
    /// <summary>
    /// Obtiene el perfil de un usuario específico (compatibilidad con documentación API)    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Perfil del usuario</returns>
    [HttpGet("/api/users/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserProfileDocumentation(int id)
    {
        return await GetUserProfile(id);
    }

    /// <summary>
    /// Actualiza el perfil de un usuario (compatibilidad con documentación API)
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="request">Datos a actualizar</param>
    /// <returns>Perfil actualizado del usuario</returns>
    [HttpPut("/api/users/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUserProfileDocumentation(int id, [FromBody] UpdateProfileRequest request)
    {
        return await UpdateUserProfile(id, request);
    }
}
