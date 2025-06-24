using Microsoft.AspNetCore.Identity;

namespace TesorosChoco.Infrastructure.Identity;

/// <summary>
/// Extended IdentityUser for application-specific user properties
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property to domain User entity
    /// </summary>
    public int? DomainUserId { get; set; }
}
