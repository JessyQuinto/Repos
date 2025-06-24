namespace TesorosChoco.Domain.Entities;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public string Slug { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public decimal? DiscountedPrice { get; set; }
    
    public string Image { get; set; } = string.Empty;
    
    public List<string> Images { get; set; } = new();
    
    public int CategoryId { get; set; }
    
    public int ProducerId { get; set; }
    
    public int Stock { get; set; }
    
    public bool Featured { get; set; } = false;
    
    public double? Rating { get; set; }
    
    // Navigation Properties
    public virtual Category Category { get; set; } = null!;
    
    public virtual Producer Producer { get; set; } = null!;
    
    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    
    // Computed Properties
    public decimal CurrentPrice => DiscountedPrice ?? Price;
    
    public bool HasDiscount => DiscountedPrice.HasValue && DiscountedPrice < Price;
    
    public bool IsInStock => Stock > 0;
}
