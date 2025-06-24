namespace TesorosChoco.Application.DTOs.Responses;

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
    public bool Success { get; set; } = true;
    public string Message { get; set; } = string.Empty;
    public object? Metadata { get; set; }
}
