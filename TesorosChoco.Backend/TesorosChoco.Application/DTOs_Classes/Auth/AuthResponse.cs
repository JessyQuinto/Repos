using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Auth;

public class AuthResponse
{
    public UserDto User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
    public int RefreshTokenExpiresIn { get; set; }
}
