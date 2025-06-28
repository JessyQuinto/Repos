using Microsoft.AspNetCore.Mvc;

namespace TesorosChoco.API.Common;

/// <summary>
/// Helper class for standardized API responses
/// Provides consistent response formats across all controllers
/// </summary>
public static class ApiResponseHelper
{
    /// <summary>
    /// Creates a standardized success response
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Response data</param>
    /// <param name="message">Success message</param>
    /// <returns>Standardized API response</returns>
    public static ActionResult<T> Success<T>(T data, string? message = null)
    {
        return new ObjectResult(new
        {
            success = true,
            message = message ?? "Operation completed successfully",
            data,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 200
        };
    }

    /// <summary>
    /// Creates a standardized created response
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    /// <param name="data">Created resource data</param>
    /// <param name="location">Location of the created resource</param>
    /// <param name="message">Success message</param>
    /// <returns>Standardized API response</returns>
    public static CreatedResult Created<T>(T data, string? location = null, string? message = null)
    {
        var responseData = new
        {
            success = true,
            message = message ?? "Resource created successfully",
            data,
            timestamp = DateTime.UtcNow
        };

        return new CreatedResult(location ?? string.Empty, responseData);
    }

    /// <summary>
    /// Creates a standardized validation error response
    /// </summary>
    /// <param name="errors">Validation errors</param>
    /// <returns>Standardized error response</returns>
    public static ActionResult ValidationError(IEnumerable<string> errors)
    {
        return new ObjectResult(new
        {
            success = false,
            message = "Validation failed",
            errors,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 400
        };
    }

    /// <summary>
    /// Creates a standardized not found response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Standardized error response</returns>
    public static ActionResult NotFound(string message = "Resource not found")
    {
        return new ObjectResult(new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 404
        };
    }

    /// <summary>
    /// Creates a standardized unauthorized response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Standardized error response</returns>
    public static ActionResult Unauthorized(string message = "Unauthorized access")
    {
        return new ObjectResult(new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 401
        };
    }

    /// <summary>
    /// Creates a standardized forbidden response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Standardized error response</returns>
    public static ActionResult Forbidden(string message = "Access forbidden")
    {
        return new ObjectResult(new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 403
        };
    }

    /// <summary>
    /// Creates a standardized internal server error response
    /// </summary>
    /// <param name="message">Error message</param>
    /// <returns>Standardized error response</returns>
    public static ActionResult InternalServerError(string message = "An internal server error occurred")
    {
        return new ObjectResult(new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        })
        {
            StatusCode = 500
        };
    }
}
