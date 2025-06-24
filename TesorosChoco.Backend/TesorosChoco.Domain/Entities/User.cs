using Microsoft.AspNetCore.Identity;

namespace TesorosChoco.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Authentication properties
    public string? RefreshToken { get; set; }
    
    public DateTime? RefreshTokenExpiryTime { get; set; }
    
    // Navigation Properties
    public virtual Cart? Cart { get; set; }
    
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    
    public string FullName => $"{FirstName} {LastName}".Trim();
}
