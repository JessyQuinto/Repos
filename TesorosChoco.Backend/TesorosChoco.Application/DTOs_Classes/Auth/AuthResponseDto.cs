using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Auth;

public class AuthResponseDto
{
    public UserDto User { get; set; } = null!;
    public string Token { get; set; } = null!;
}
