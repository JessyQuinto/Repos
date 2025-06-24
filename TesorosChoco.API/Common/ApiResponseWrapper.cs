using System.Text.Json.Serialization;

namespace TesorosChoco.API.Common;

/// <summary>
/// Wrapper para todas las respuestas de la API según especificación dotnet-integration.md
/// </summary>
/// <typeparam name="T">Tipo de datos de la respuesta</typeparam>
public class ApiResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("metadata")]
    public ApiMetadata Metadata { get; set; } = new();

    public ApiResponse()
    {
    }

    public ApiResponse(T? data, bool success = true, string message = "")
    {
        Data = data;
        Success = success;
        Message = message;
        Metadata = new ApiMetadata();
    }

    public static ApiResponse<T> SuccessResponse(T? data, string message = "Operation successful")
    {
        return new ApiResponse<T>(data, true, message);
    }

    public static ApiResponse<T> ErrorResponse(string message, T? data = default)
    {
        return new ApiResponse<T>(data, false, message);
    }
}

/// <summary>
/// Wrapper para respuestas sin datos específicos
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public ApiResponse() : base()
    {
    }

    public ApiResponse(bool success, string message) : base(null, success, message)
    {
    }

    public static ApiResponse SuccessResponse(string message = "Operation successful")
    {
        return new ApiResponse(true, message);
    }

    public static new ApiResponse ErrorResponse(string message)
    {
        return new ApiResponse(false, message);
    }
}

/// <summary>
/// Metadatos de la respuesta API según especificación
/// </summary>
public class ApiMetadata
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("correlationId")]
    public string CorrelationId { get; set; }

    [JsonPropertyName("version")]
    public string Version { get; set; }

    public ApiMetadata()
    {
        Timestamp = DateTime.UtcNow;
        CorrelationId = Guid.NewGuid().ToString("N")[..8]; // Primeros 8 caracteres
        Version = "1.0";
    }
}
