using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;

namespace TesorosChoco.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<GenericResponse> LogoutAsync(RefreshTokenRequest request);
    Task<GenericResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<GenericResponse> ResetPasswordAsync(ResetPasswordRequest request);
}
