namespace TesorosChoco.Application.DTOs.Requests;

public class CreateProductRequest
{
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
}

public class UpdateProductRequest
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
}
