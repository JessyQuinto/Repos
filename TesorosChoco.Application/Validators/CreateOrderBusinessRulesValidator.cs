using FluentValidation;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Validators;

/// <summary>
/// Business rules validator for order creation
/// Ensures all business constraints are met before order processing
/// </summary>
public class CreateOrderBusinessRulesValidator : AbstractValidator<CreateOrderRequest>
{
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;

    public CreateOrderBusinessRulesValidator(
        IProductRepository productRepository,
        IUserRepository userRepository,
        ICategoryRepository categoryRepository)
    {
        _productRepository = productRepository;
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;

        RuleFor(x => x.UserId)
            .MustAsync(UserMustExist).WithMessage("User does not exist")
            .MustAsync(UserMustBeActive).WithMessage("User account is not active");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item")
            .Must(items => items.Count <= 50).WithMessage("Order cannot contain more than 50 different items");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .MustAsync(ProductMustExist).WithMessage("Product does not exist")
                .MustAsync(ProductMustBeActive).WithMessage("Product is not available for purchase")
                .MustAsync(ProductMustBeInStock).WithMessage("Product is out of stock");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Maximum quantity per item is 100");

            item.RuleFor(x => x)
                .MustAsync(SufficientStockAvailable).WithMessage("Insufficient stock available");
        });

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required")
            .ChildRules(address =>
            {
                address.RuleFor(x => x.Name).NotEmpty().WithMessage("Recipient name is required");
                address.RuleFor(x => x.Address).NotEmpty().WithMessage("Address is required");
                address.RuleFor(x => x.City).NotEmpty().WithMessage("City is required");
                address.RuleFor(x => x.Phone).NotEmpty().WithMessage("Phone number is required");
            });

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Payment method is required")
            .Must(BeValidPaymentMethod).WithMessage("Invalid payment method");
    }

    private async Task<bool> UserMustExist(int userId, CancellationToken cancellationToken)
    {
        var user = await _productRepository.GetByIdAsync(userId);
        return user != null;
    }

    private async Task<bool> UserMustBeActive(int userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user != null; // Add IsActive property to User entity if needed
    }

    private async Task<bool> ProductMustExist(int productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product != null;
    }

    private async Task<bool> ProductMustBeActive(int productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product?.IsAvailableForPurchase == true;
    }

    private async Task<bool> ProductMustBeInStock(int productId, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        return product?.IsInStock == true;
    }    private async Task<bool> SufficientStockAvailable(OrderItemRequest item, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        return product != null && product.Stock >= item.Quantity;
    }

    private bool BeValidPaymentMethod(string paymentMethod)
    {
        var validMethods = new[] { "CreditCard", "DebitCard", "PayPal", "BankTransfer", "Cash" };
        return validMethods.Contains(paymentMethod, StringComparer.OrdinalIgnoreCase);
    }    /// <summary>
    /// Validates product-specific business rules for order creation
    /// </summary>
    /// <param name="product">Product to validate</param>
    /// <param name="quantity">Requested quantity</param>
    /// <returns>Task representing the validation operation</returns>
    public Task ValidateProductForOrderAsync(Product product, int quantity)
    {
        if (product == null)
            throw new ArgumentException("Product does not exist");

        if (!product.IsAvailableForPurchase)
            throw new InvalidOperationException($"Product '{product.Name}' is not available for purchase");

        if (!product.IsInStock)
            throw new InvalidOperationException($"Product '{product.Name}' is out of stock");

        if (product.Stock < quantity)
            throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Stock}, Requested: {quantity}");

        // Additional business rules can be added here
        // For example: minimum order quantities, restricted products, etc.
        
        return Task.CompletedTask;
    }
}
