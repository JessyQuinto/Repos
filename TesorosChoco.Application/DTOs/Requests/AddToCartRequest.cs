using System.ComponentModel.DataAnnotations;

namespace TesorosChoco.Application.DTOs.Requests;

/// <summary>
/// Request para agregar un item al carrito
/// </summary>
public class AddToCartRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Product ID must be greater than 0")]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
    public int Quantity { get; set; }
}
