using TesorosChoco.Domain.Enums;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Domain.Entities;

public class Order : BaseEntity
{
    public int UserId { get; set; }
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    
    public ShippingAddress ShippingAddress { get; set; } = null!;
    
    public string PaymentMethod { get; set; } = string.Empty;
    
    public decimal Total { get; set; }
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
    
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    
    // Computed Properties
    public int TotalItems => Items.Sum(item => item.Quantity);
}
