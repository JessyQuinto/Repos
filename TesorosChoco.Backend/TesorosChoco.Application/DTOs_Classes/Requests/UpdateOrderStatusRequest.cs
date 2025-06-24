using TesorosChoco.Domain.Enums;

namespace TesorosChoco.Application.DTOs.Requests;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}
