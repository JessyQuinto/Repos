namespace TesorosChoco.Application.DTOs.Requests;

public class CreateOrderRequest
{
    public int UserId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public ShippingAddressRequest ShippingAddress { get; set; } = new();
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class ShippingAddressRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = string.Empty;
}
