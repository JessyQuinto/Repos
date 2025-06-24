using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Token service wrapper that adapts IJwtTokenService to ITokenService interface
/// Provides a clean abstraction layer for authentication operations
/// </summary>
public class TokenService : ITokenService
{
    private readonly IJwtTokenService _jwtTokenService;

    public TokenService(IJwtTokenService jwtTokenService)
    {
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
    }

    public string GenerateAccessToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        // For now, assign default role. In future, implement proper role management
        var roles = new List<string> { "User" };
        
        return _jwtTokenService.GenerateAccessToken(user.Id, user.Email, roles);
    }

    public string GenerateRefreshToken()
    {
        return _jwtTokenService.GenerateRefreshToken();
    }

    public bool ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        return _jwtTokenService.ValidateToken(token);
    }
}
