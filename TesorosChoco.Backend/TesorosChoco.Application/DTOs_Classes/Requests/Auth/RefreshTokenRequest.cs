using System.ComponentModel.DataAnnotations;

namespace TesorosChoco.Application.DTOs.Requests.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
