using MediatR;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;

namespace TesorosChoco.Application.Commands.Products;

/// <summary>
/// Command to create a new product
/// </summary>
public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductDto>;

/// <summary>
/// Command to update an existing product
/// </summary>
public record UpdateProductCommand(int Id, UpdateProductRequest Request) : IRequest<ProductDto?>;

/// <summary>
/// Command to delete a product
/// </summary>
public record DeleteProductCommand(int Id) : IRequest<bool>;

/// <summary>
/// Command to update product stock
/// </summary>
public record UpdateProductStockCommand(int ProductId, int NewStock) : IRequest<bool>;
