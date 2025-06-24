using FluentValidation;
using TesorosChoco.Application.Commands.Products;
using TesorosChoco.Application.Queries.Products;

namespace TesorosChoco.Application.Validators.Products;

/// <summary>
/// Validator for CreateProductCommand
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Request.Slug)
            .NotEmpty().WithMessage("Product slug is required")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MaximumLength(200).WithMessage("Slug cannot exceed 200 characters");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Request.DiscountedPrice)
            .GreaterThan(0).WithMessage("Discounted price must be greater than 0")
            .LessThan(x => x.Request.Price).WithMessage("Discounted price must be less than regular price")
            .When(x => x.Request.DiscountedPrice.HasValue);

        RuleFor(x => x.Request.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be specified");

        RuleFor(x => x.Request.ProducerId)
            .GreaterThan(0).WithMessage("Producer ID must be specified");

        RuleFor(x => x.Request.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.Request.Image)
            .NotEmpty().WithMessage("Main product image is required");
    }
}

/// <summary>
/// Validator for UpdateProductCommand
/// </summary>
public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be specified");

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

        RuleFor(x => x.Request.Slug)
            .NotEmpty().WithMessage("Product slug is required")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MaximumLength(200).WithMessage("Slug cannot exceed 200 characters");

        RuleFor(x => x.Request.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.Request.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.Request.DiscountedPrice)
            .GreaterThan(0).WithMessage("Discounted price must be greater than 0")
            .LessThan(x => x.Request.Price).WithMessage("Discounted price must be less than regular price")
            .When(x => x.Request.DiscountedPrice.HasValue);

        RuleFor(x => x.Request.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be specified");

        RuleFor(x => x.Request.ProducerId)
            .GreaterThan(0).WithMessage("Producer ID must be specified");

        RuleFor(x => x.Request.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");

        RuleFor(x => x.Request.Image)
            .NotEmpty().WithMessage("Main product image is required");
    }
}

/// <summary>
/// Validator for GetAllProductsQuery
/// </summary>
public class GetAllProductsQueryValidator : AbstractValidator<GetAllProductsQuery>
{
    public GetAllProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Category ID must be greater than 0")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.ProducerId)
            .GreaterThan(0).WithMessage("Producer ID must be greater than 0")
            .When(x => x.ProducerId.HasValue);
    }
}

/// <summary>
/// Validator for GetProductByIdQuery
/// </summary>
public class GetProductByIdQueryValidator : AbstractValidator<GetProductByIdQuery>
{
    public GetProductByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Product ID must be greater than 0");
    }
}
