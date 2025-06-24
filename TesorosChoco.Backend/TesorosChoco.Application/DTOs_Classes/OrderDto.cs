using System;
using System.Collections.Generic;
using TesorosChoco.Domain.Enums;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Application.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public ShippingAddress ShippingAddress { get; set; } = null!;
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int TotalItems { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
