using System.Text.Json.Serialization;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Responses;

/// <summary>
/// Respuesta de autenticación según especificación dotnet-integration.md
/// </summary>
public class AuthResponse
{
    [JsonPropertyName("user")]
    public UserDto User { get; set; } = new();

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = string.Empty;

    [JsonPropertyName("tokenType")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expiresIn")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refreshTokenExpiresIn")]
    public int RefreshTokenExpiresIn { get; set; }

    public AuthResponse()
    {
    }

    public AuthResponse(UserDto user, string accessToken, string refreshToken, int expiresIn = 3600, int refreshTokenExpiresIn = 604800)
    {
        User = user;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
        RefreshTokenExpiresIn = refreshTokenExpiresIn;
    }
}
