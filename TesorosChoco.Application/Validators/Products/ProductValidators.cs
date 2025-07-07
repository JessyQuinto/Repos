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
        this.AddNameValidation(x => x.Request.Name);
        this.AddSlugValidation(x => x.Request.Slug);
        this.AddDescriptionValidation(x => x.Request.Description);
        this.AddPriceValidation(x => x.Request.Price, x => x.Request.DiscountedPrice);
        this.AddCategoryValidation(x => x.Request.CategoryId);
        this.AddProducerValidation(x => x.Request.ProducerId);
        this.AddStockValidation(x => x.Request.Stock);
        this.AddImageValidation(x => x.Request.Image);
        this.AddStatusValidation(x => x.Request.Status);
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

        // Reuse common validation rules
        this.AddNameValidation(x => x.Request.Name);
        this.AddSlugValidation(x => x.Request.Slug);
        this.AddDescriptionValidation(x => x.Request.Description);
        this.AddPriceValidation(x => x.Request.Price, x => x.Request.DiscountedPrice);
        this.AddCategoryValidation(x => x.Request.CategoryId);
        this.AddProducerValidation(x => x.Request.ProducerId);
        this.AddStockValidation(x => x.Request.Stock);
        this.AddImageValidation(x => x.Request.Image);
        this.AddStatusValidation(x => x.Request.Status);
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
