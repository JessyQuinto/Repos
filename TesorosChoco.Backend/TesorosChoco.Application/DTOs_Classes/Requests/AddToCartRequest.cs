namespace TesorosChoco.Application.DTOs.Requests;

public class AddToCartRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}
