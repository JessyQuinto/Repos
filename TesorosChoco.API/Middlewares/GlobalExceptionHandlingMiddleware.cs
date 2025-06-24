using System.Net;
using System.Text.Json;

namespace TesorosChoco.API.Middlewares;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = "An error occurred",
            status = (int)HttpStatusCode.InternalServerError,
            detail = exception.Message,
            instance = context.Request.Path.Value,
            traceId = context.TraceIdentifier
        };

        switch (exception)
        {
            case ArgumentException:
                response = response with 
                { 
                    title = "Validation Error",
                    status = (int)HttpStatusCode.BadRequest 
                };
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;
            case UnauthorizedAccessException:
                response = response with 
                { 
                    title = "Unauthorized",
                    status = (int)HttpStatusCode.Unauthorized 
                };
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;
            case KeyNotFoundException:
                response = response with 
                { 
                    title = "Not Found",
                    status = (int)HttpStatusCode.NotFound 
                };
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
