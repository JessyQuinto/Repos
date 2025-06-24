namespace TesorosChoco.API.Common;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public ApiMetadata? Metadata { get; set; }

    public ApiResponse()
    {
        Metadata = new ApiMetadata();
    }

    public ApiResponse(T data, string message = "Operation successful")
    {
        Data = data;
        Message = message;
        Metadata = new ApiMetadata();
    }

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>(data, message);
    }

    public static ApiResponse<T> ErrorResult(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Success = false,
            Message = message,
            Metadata = new ApiMetadata()
        };
    }
}

public class ApiMetadata
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    public string Version { get; set; } = "1.0";
}
