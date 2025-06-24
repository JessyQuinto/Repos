using MediatR;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.Queries.Products;

public record SearchProductsQuery(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int? ProducerId = null,
    bool? Featured = null,
    int Limit = 10,
    int Offset = 0
) : IRequest<SearchProductsResponse>;

public class SearchProductsResponse
{
    public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Limit { get; set; }
}
