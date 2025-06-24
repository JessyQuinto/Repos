using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Responses;

public class AuthResponse
{
    public UserDto User { get; set; } = new();
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
