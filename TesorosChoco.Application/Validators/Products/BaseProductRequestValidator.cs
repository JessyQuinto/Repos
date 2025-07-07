using FluentValidation;
using System.Linq.Expressions;
using TesorosChoco.Domain.Enums;

namespace TesorosChoco.Application.Validators.Products;

/// <summary>
/// Common validation rules for product requests to avoid code duplication
/// </summary>
public static class ProductValidationRules
{
    public static void AddNameValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> nameSelector)
    {
        validator.RuleFor(nameSelector)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");
    }

    public static void AddSlugValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> slugSelector)
    {
        validator.RuleFor(slugSelector)
            .NotEmpty().WithMessage("Product slug is required")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens")
            .MaximumLength(200).WithMessage("Slug cannot exceed 200 characters");
    }

    public static void AddDescriptionValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> descriptionSelector)
    {
        validator.RuleFor(descriptionSelector)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");
    }    public static void AddPriceValidation<T>(this AbstractValidator<T> validator, 
        Expression<Func<T, decimal>> priceSelector, 
        Expression<Func<T, decimal?>> discountedPriceSelector)
    {
        validator.RuleFor(priceSelector)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        validator.RuleFor(discountedPriceSelector)
            .GreaterThan(0).WithMessage("Discounted price must be greater than 0")
            .Must((obj, discountedPrice) => !discountedPrice.HasValue || discountedPrice.Value < priceSelector.Compile()(obj))
            .WithMessage("Discounted price must be less than regular price")
            .When(x => discountedPriceSelector.Compile()(x).HasValue);
    }

    public static void AddCategoryValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, int>> categoryIdSelector)
    {
        validator.RuleFor(categoryIdSelector)
            .GreaterThan(0).WithMessage("Category ID must be specified");
    }

    public static void AddProducerValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, int>> producerIdSelector)
    {
        validator.RuleFor(producerIdSelector)
            .GreaterThan(0).WithMessage("Producer ID must be specified");
    }

    public static void AddStockValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, int>> stockSelector)
    {
        validator.RuleFor(stockSelector)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative");
    }

    public static void AddImageValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, string>> imageSelector)
    {
        validator.RuleFor(imageSelector)
            .NotEmpty().WithMessage("Main product image is required");
    }

    public static void AddStatusValidation<T>(this AbstractValidator<T> validator, Expression<Func<T, ProductStatus>> statusSelector)
    {
        validator.RuleFor(statusSelector)
            .IsInEnum().WithMessage("Invalid product status")
            .NotEqual(ProductStatus.OutOfStock).WithMessage("Product status cannot be set to OutOfStock manually");
    }
}
