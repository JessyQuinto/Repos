using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TesorosChoco.API.Controllers;

/// <summary>
/// Controlador base que proporciona funcionalidad común para todos los controladores
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado desde el token JWT
    /// </summary>
    /// <returns>ID del usuario autenticado</returns>
    /// <exception cref="UnauthorizedAccessException">Cuando no se puede obtener el ID del usuario</exception>
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                         ?? User.FindFirst("sub")?.Value 
                         ?? User.FindFirst("userId")?.Value;
        
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Unable to retrieve user ID from token");
        }
        
        return userId;
    }

    /// <summary>
    /// Obtiene el ID del usuario autenticado de forma segura
    /// </summary>
    /// <returns>ID del usuario si está autenticado, null en caso contrario</returns>
    protected int? GetCurrentUserIdSafe()
    {
        try
        {
            return GetCurrentUserId();
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>
    /// Verifica si el usuario actual es administrador
    /// </summary>
    /// <returns>True si el usuario es administrador</returns>
    protected bool IsCurrentUserAdmin()
    {
        return User.IsInRole("Admin");
    }

    /// <summary>
    /// Verifica si el usuario actual puede acceder a un recurso específico de usuario
    /// </summary>
    /// <param name="targetUserId">ID del usuario objetivo</param>
    /// <returns>True si puede acceder (es el mismo usuario o es administrador)</returns>
    protected bool CanAccessUserResource(int targetUserId)
    {
        var currentUserId = GetCurrentUserIdSafe();
        return currentUserId.HasValue && (currentUserId.Value == targetUserId || IsCurrentUserAdmin());
    }
}
