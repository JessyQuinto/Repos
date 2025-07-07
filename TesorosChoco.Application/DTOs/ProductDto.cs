using TesorosChoco.Domain.Enums;

namespace TesorosChoco.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string Image { get; set; } = string.Empty;
    public string[] Images { get; set; } = Array.Empty<string>();
    public int CategoryId { get; set; }
    public int ProducerId { get; set; }
    public int Stock { get; set; }
    public bool Featured { get; set; }
    public double? Rating { get; set; }
    public ProductStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Computed Properties para el Frontend
    public decimal CurrentPrice => DiscountedPrice ?? Price;
    public bool HasDiscount => DiscountedPrice.HasValue && DiscountedPrice < Price;
    public bool IsInStock => Stock > 0;
    public bool IsAvailableForPurchase => Status == ProductStatus.Active && IsInStock;
}
