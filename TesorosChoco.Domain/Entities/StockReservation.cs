using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Entities;

/// <summary>
/// Stock reservation entity for temporary inventory hold during checkout process
/// </summary>
public class StockReservation : BaseEntity
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? SessionId { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Navigation Properties
    public virtual Product Product { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    
    // Business Logic
    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    
    public void Expire()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
