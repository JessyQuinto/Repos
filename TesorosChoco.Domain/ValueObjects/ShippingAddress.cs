namespace TesorosChoco.Domain.ValueObjects;

public class ShippingAddress
{
    public string Name { get; set; } = string.Empty;
    
    public string Address { get; set; } = string.Empty;
    
    public string City { get; set; } = string.Empty;
    
    public string ZipCode { get; set; } = string.Empty;
    
    public string Phone { get; set; } = string.Empty;
    
    public string FullAddress => $"{Name} - {Address}, {City} {ZipCode}";
}
