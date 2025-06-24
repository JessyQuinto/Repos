using MediatR;
using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.Queries.Products;

/// <summary>
/// Query to get all products with optional filtering
/// </summary>
public record GetAllProductsQuery : IRequest<IEnumerable<ProductDto>>
{
    public bool? Featured { get; init; }
    public int? CategoryId { get; init; }
    public int? ProducerId { get; init; }
    public string? SearchTerm { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(int Id) : IRequest<ProductDto?>;

/// <summary>
/// Query to get featured products
/// </summary>
public record GetFeaturedProductsQuery(int Count = 10) : IRequest<IEnumerable<ProductDto>>;

/// <summary>
/// Query to get a product by slug
/// </summary>
public record GetProductBySlugQuery(string Slug) : IRequest<ProductDto?>;
