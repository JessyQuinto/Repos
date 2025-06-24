namespace TesorosChoco.Domain.Entities;

public class CartItem : BaseEntity
{
    public int CartId { get; set; }
    
    public int ProductId { get; set; }
    
    public int Quantity { get; set; }
    
    public decimal Price { get; set; } // Precio al momento de agregar
    
    // Navigation Properties
    public virtual Cart Cart { get; set; } = null!;
    
    public virtual Product Product { get; set; } = null!;
    
    // Computed Properties
    public decimal Subtotal => Price * Quantity;
}
