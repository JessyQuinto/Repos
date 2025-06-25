using FluentValidation;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Application.Validators.Orders;

/// <summary>
/// Advanced business rules validator for order value and quantity limits
/// Ensures orders meet minimum/maximum requirements and business policies
/// </summary>
public class OrderValueAndLimitsValidator : AbstractValidator<CreateOrderRequest>
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IOrderRepository _orderRepository;

    // Business configuration - in a real application, these would come from configuration or database
    private const decimal MIN_ORDER_VALUE = 25.00m; // Minimum order value
    private const decimal MAX_ORDER_VALUE = 5000.00m; // Maximum order value per order
    private const int MAX_ITEMS_PER_ORDER = 50; // Maximum different items per order
    private const int MAX_QUANTITY_PER_ITEM = 20; // Maximum quantity for any single item
    private const int MAX_ORDERS_PER_DAY_PER_USER = 5; // Maximum orders per user per day

    public OrderValueAndLimitsValidator(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IOrderRepository orderRepository)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _orderRepository = orderRepository;

        RuleFor(x => x)
            .MustAsync(MeetMinimumOrderValue).WithMessage($"Order value must be at least ${MIN_ORDER_VALUE:F2}")
            .MustAsync(NotExceedMaximumOrderValue).WithMessage($"Order value cannot exceed ${MAX_ORDER_VALUE:F2}")
            .MustAsync(NotExceedDailyOrderLimit).WithMessage($"Maximum {MAX_ORDERS_PER_DAY_PER_USER} orders per day allowed");

        RuleFor(x => x.Items)
            .Must(items => items.Count <= MAX_ITEMS_PER_ORDER)
            .WithMessage($"Maximum {MAX_ITEMS_PER_ORDER} different items allowed per order");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.Quantity)
                .LessThanOrEqualTo(MAX_QUANTITY_PER_ITEM)
                .WithMessage($"Maximum quantity per item is {MAX_QUANTITY_PER_ITEM}");

            item.RuleFor(x => x)
                .MustAsync(ValidateCategoryRestrictions)
                .WithMessage("This product cannot be ordered with the current selection due to category restrictions");
        });
    }

    private async Task<bool> MeetMinimumOrderValue(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var totalValue = await CalculateOrderValue(request, cancellationToken);
        return totalValue >= MIN_ORDER_VALUE;
    }

    private async Task<bool> NotExceedMaximumOrderValue(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var totalValue = await CalculateOrderValue(request, cancellationToken);
        return totalValue <= MAX_ORDER_VALUE;
    }

    private async Task<bool> NotExceedDailyOrderLimit(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var ordersToday = await _orderRepository.GetOrdersByUserAndDateRangeAsync(
            request.UserId, today, today.AddDays(1));
        
        return ordersToday.Count() < MAX_ORDERS_PER_DAY_PER_USER;
    }

    private async Task<bool> ValidateCategoryRestrictions(OrderItemRequest item, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(item.ProductId);
        if (product?.CategoryId == null) return true;

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        if (category == null) return true;

        // Example business rule: Certain categories cannot be mixed
        // This would be configured based on business requirements
        // For example: Dairy products with certain chocolates, temperature-sensitive items, etc.
        
        // For demonstration, let's say categories with IDs 1 and 2 cannot be ordered together
        if (category.Id == 1 || category.Id == 2)
        {
            // Check if there are conflicting categories in the order
            // This is a simplified example - in reality, you'd have more complex rules
            return true; // Simplified for now
        }

        return true;
    }

    private async Task<decimal> CalculateOrderValue(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        decimal total = 0;
        
        foreach (var item in request.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                total += product.Price * item.Quantity;
            }
        }
        
        return total;
    }
}
