using Microsoft.AspNetCore.Identity;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Identity;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Password service implementation using ASP.NET Core Identity PasswordHasher
/// Provides secure password hashing and verification using industry-standard algorithms
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

    public PasswordService(IPasswordHasher<ApplicationUser> passwordHasher)
    {
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        // Use a dummy user for hashing - the user object is not used by the hasher
        var dummyUser = new ApplicationUser();
        return _passwordHasher.HashPassword(dummyUser, password);
    }

    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
            
        if (string.IsNullOrWhiteSpace(hash))
            return false;

        try
        {
            // Use a dummy user for verification - the user object is not used by the hasher
            var dummyUser = new ApplicationUser();
            var result = _passwordHasher.VerifyHashedPassword(dummyUser, hash, password);
            
            return result == PasswordVerificationResult.Success || 
                   result == PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch
        {
            return false;
        }
    }

    public bool NeedsRehashing(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            return true;

        try
        {
            // We can only determine if rehashing is needed during verification
            // This is a simplified check - in practice, you'd call VerifyPassword and check for SuccessRehashNeeded
            // For now, we'll return false as ASP.NET Core Identity handles this internally
            return false;
        }
        catch
        {
            return true;
        }
    }
}
