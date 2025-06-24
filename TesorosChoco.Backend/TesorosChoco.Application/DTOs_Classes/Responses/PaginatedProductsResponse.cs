using System.Collections.Generic;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.DTOs.Responses;

public class PaginatedProductsResponse
{
    public List<ProductDto> Products { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}
