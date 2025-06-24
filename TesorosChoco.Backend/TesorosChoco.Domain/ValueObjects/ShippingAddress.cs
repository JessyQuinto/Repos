namespace TesorosChoco.Domain.ValueObjects;

public class ShippingAddress
{
    public string Street { get; set; } = string.Empty;
    
    public string City { get; set; } = string.Empty;
    
    public string State { get; set; } = string.Empty;
    
    public string ZipCode { get; set; } = string.Empty;
    
    public string Country { get; set; } = string.Empty;
    
    public string FullAddress => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
