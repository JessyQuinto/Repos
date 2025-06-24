using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TesorosChoco.Infrastructure.Identity;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Service for managing user authentication and identity operations
/// Bridges between ASP.NET Core Identity and domain layer
/// </summary>
public interface IIdentityService
{
    Task<(bool Success, string[] Errors)> CreateUserAsync(string firstName, string lastName, string email, string password, string? phone = null, string? address = null);
    Task<(bool Success, int UserId, string Email, IList<string> Roles)> AuthenticateAsync(string email, string password);
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByIdAsync(int userId);
    Task<bool> UpdateUserAsync(int userId, string firstName, string lastName, string? phone = null, string? address = null);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<bool> ResetPasswordAsync(string email, string token, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(string email);
    Task<bool> ConfirmEmailAsync(int userId, string token);
    Task<string> GenerateEmailConfirmationTokenAsync(int userId);
}

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<(bool Success, string[] Errors)> CreateUserAsync(
        string firstName, 
        string lastName, 
        string email, 
        string password, 
        string? phone = null, 
        string? address = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Phone = phone,
            Address = address,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailConfirmed = false // Set to true in development, false in production
        };

        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            // Add default user role
            await _userManager.AddToRoleAsync(user, "User");
            return (true, Array.Empty<string>());
        }

        return (false, result.Errors.Select(e => e.Description).ToArray());
    }

    public async Task<(bool Success, int UserId, string Email, IList<string> Roles)> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return (false, 0, string.Empty, new List<string>());
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return (true, user.Id, user.Email!, roles);
        }

        return (false, 0, string.Empty, new List<string>());
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(int userId)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<bool> UpdateUserAsync(int userId, string firstName, string lastName, string? phone = null, string? address = null)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        user.FirstName = firstName;
        user.LastName = lastName;
        user.Phone = phone;
        user.Address = address;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return false;

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.Succeeded;
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            throw new InvalidOperationException("User not found");

        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<bool> ConfirmEmailAsync(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return false;

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded;
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new InvalidOperationException("User not found");

        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }
}
