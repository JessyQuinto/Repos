namespace TesorosChoco.Domain.Entities;

public class Producer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;
    
    public string Image { get; set; } = string.Empty;
    
    public bool Featured { get; set; } = false;
    
    public int? FoundedYear { get; set; }
    
    // Navigation Properties
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
