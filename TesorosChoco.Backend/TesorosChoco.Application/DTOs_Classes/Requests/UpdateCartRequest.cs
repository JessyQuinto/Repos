using System.Collections.Generic;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Requests;

public class UpdateCartRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
}
