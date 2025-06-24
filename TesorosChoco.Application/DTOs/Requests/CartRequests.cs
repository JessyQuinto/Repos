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
