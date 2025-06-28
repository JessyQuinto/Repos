using System.Net;
using System.Text.Json;
using FluentValidation;

namespace TesorosChoco.API.Middlewares;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// Implements comprehensive error handling following Azure best practices
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message} at {Path}", 
                ex.Message, context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, title, detail) = GetErrorDetails(exception, context);
        
        var response = new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path.Value,
            traceId = context.TraceIdentifier,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = (int)statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static (HttpStatusCode statusCode, string title, string detail) GetErrorDetails(Exception exception, HttpContext context)
    {
        return exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                "Validation Error",
                string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage))
            ),
            ArgumentNullException => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                "Required data is missing"
            ),
            ArgumentException => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                "Invalid request data"
            ),
            UnauthorizedAccessException => (
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                "Access denied"
            ),
            KeyNotFoundException => (
                HttpStatusCode.NotFound,
                "Not Found",
                "The requested resource was not found"
            ),
            InvalidOperationException => (
                HttpStatusCode.Conflict,
                "Conflict",
                "The operation cannot be completed due to the current state"
            ),
            TimeoutException => (
                HttpStatusCode.RequestTimeout,
                "Request Timeout",
                "The request timed out"
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred"
            )
        };
    }
}
