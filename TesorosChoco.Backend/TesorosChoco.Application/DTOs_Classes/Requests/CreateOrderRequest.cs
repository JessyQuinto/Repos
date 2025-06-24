using System.Collections.Generic;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Application.DTOs.Requests;

public class CreateOrderRequest
{
    public int UserId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
}
