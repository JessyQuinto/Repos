using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(int id);
}

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public interface IRefreshTokenService
{
    Task<string> CreateRefreshTokenAsync(int userId);
    Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeRefreshTokenAsync(int userId, string refreshToken);
    Task RevokeAllRefreshTokensAsync(int userId);
    Task CleanupExpiredTokensAsync();
}
