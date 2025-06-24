namespace TesorosChoco.Domain.Entities;

public class Cart : BaseEntity
{
    public int UserId { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    
    public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    
    // Computed Properties
    public decimal Total => Items.Sum(item => item.Price * item.Quantity);
    
    public int TotalItems => Items.Sum(item => item.Quantity);
}
