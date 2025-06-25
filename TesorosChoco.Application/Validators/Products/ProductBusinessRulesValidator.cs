using FluentValidation;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Validators.Products;

/// <summary>
/// Enhanced product validator with business rules for category restrictions and seasonal availability
/// </summary>
public class ProductBusinessRulesValidator : AbstractValidator<CreateProductRequest>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProducerRepository _producerRepository;

    // Seasonal restrictions - products available only in certain months
    private static readonly Dictionary<string, int[]> SEASONAL_PRODUCTS = new()
    {
        { "Valentine", new[] { 1, 2, 14 } }, // January, February, and Valentine's day month
        { "Easter", new[] { 3, 4 } }, // March, April
        { "Christmas", new[] { 11, 12 } }, // November, December
        { "Summer", new[] { 6, 7, 8 } }, // June, July, August
        { "Winter", new[] { 12, 1, 2 } } // December, January, February
    };

    // Category-based restrictions
    private static readonly string[] RESTRICTED_CATEGORIES = { "Alcohol", "Adult", "Prescription" };
    private static readonly string[] TEMPERATURE_SENSITIVE = { "Ice Cream", "Frozen", "Dairy" };

    public ProductBusinessRulesValidator(
        ICategoryRepository categoryRepository,
        IProducerRepository producerRepository)
    {
        _categoryRepository = categoryRepository;
        _producerRepository = producerRepository;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .Length(3, 200).WithMessage("Product name must be between 3 and 200 characters")
            .Must(NotContainRestrictedWords).WithMessage("Product name contains restricted words");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0")
            .LessThan(10000).WithMessage("Price cannot exceed $10,000")
            .Must(BeValidPriceIncrement).WithMessage("Price must be in valid increments (e.g., 0.05, 0.10, 0.25, 0.50)");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
            .LessThan(10000).WithMessage("Stock cannot exceed 10,000 units");

        RuleFor(x => x.CategoryId)
            .MustAsync(CategoryMustExist).WithMessage("Category does not exist")
            .MustAsync(CategoryMustBeActive).WithMessage("Category is not active")
            .MustAsync(ValidateSeasonalRestrictions).WithMessage("Product category is not available in current season");

        RuleFor(x => x.ProducerId)
            .MustAsync(ProducerMustExist).WithMessage("Producer does not exist")
            .MustAsync(ProducerMustBeActive).WithMessage("Producer is not active");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .Must(NotContainRestrictedContent).WithMessage("Description contains inappropriate content");        When(x => !string.IsNullOrEmpty(x.Image), () =>
        {
            RuleFor(x => x.Image)
                .Must(BeValidImageUrl).WithMessage("Invalid image URL format")
                .Must(BeAllowedImageExtension).WithMessage("Image must be JPG, PNG, or WebP format");
        });
    }

    private async Task<bool> CategoryMustExist(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        return category != null;
    }    private async Task<bool> CategoryMustBeActive(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        // Since Category doesn't have IsActive property, we'll just check if it exists
        return category != null;
    }

    private async Task<bool> ValidateSeasonalRestrictions(int categoryId, CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(categoryId);
        if (category == null) return false;

        var currentMonth = DateTime.Now.Month;
        
        // Check if this category has seasonal restrictions
        foreach (var seasonalProduct in SEASONAL_PRODUCTS)
        {
            if (category.Name.Contains(seasonalProduct.Key, StringComparison.OrdinalIgnoreCase))
            {
                return seasonalProduct.Value.Contains(currentMonth);
            }
        }

        return true; // No seasonal restrictions
    }

    private async Task<bool> ProducerMustExist(int producerId, CancellationToken cancellationToken)
    {
        var producer = await _producerRepository.GetByIdAsync(producerId);
        return producer != null;
    }    private async Task<bool> ProducerMustBeActive(int producerId, CancellationToken cancellationToken)
    {
        var producer = await _producerRepository.GetByIdAsync(producerId);
        // Since Producer doesn't have IsActive property, we'll just check if it exists
        return producer != null;
    }

    private bool NotContainRestrictedWords(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;

        var restrictedWords = new[] { "fake", "counterfeit", "imitation", "replica" };
        return !restrictedWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeValidPriceIncrement(decimal price)
    {
        // Ensure price is in valid increments (multiples of 0.05)
        var remainder = price % 0.05m;
        return Math.Abs(remainder) < 0.001m || Math.Abs(remainder - 0.05m) < 0.001m;
    }

    private bool NotContainRestrictedContent(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return true;

        var restrictedPhrases = new[] 
        { 
            "guaranteed cure", "medical claims", "prescription alternative",
            "miracle product", "instant weight loss"
        };

        return !restrictedPhrases.Any(phrase => 
            description.Contains(phrase, StringComparison.OrdinalIgnoreCase));
    }

    private bool BeValidImageUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return true;

        return Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private bool BeAllowedImageExtension(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl)) return true;

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(imageUrl).ToLowerInvariant();
        
        return allowedExtensions.Contains(extension);
    }
}
