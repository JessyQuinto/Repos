namespace TesorosChoco.Application.DTOs.Requests;

public class UpdateCartRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemRequest> Items { get; set; } = new();
    public decimal Total { get; set; }
}

public class CartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}

public class AddCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}

public class CheckoutCartRequest
{
    public string PaymentMethod { get; set; } = string.Empty;
    public ShippingAddressRequest ShippingAddress { get; set; } = new();
    public string? Notes { get; set; }
}
