using FluentValidation;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Validators.Orders;

/// <summary>
/// Validator for time and region-based business rules
/// Handles delivery time windows, regional restrictions, and availability schedules
/// </summary>
public class OrderTimeAndRegionValidator : AbstractValidator<CreateOrderRequest>
{
    // Business hours configuration
    private static readonly TimeSpan BUSINESS_START = new(8, 0, 0); // 8:00 AM
    private static readonly TimeSpan BUSINESS_END = new(18, 0, 0);   // 6:00 PM
    private static readonly DayOfWeek[] BUSINESS_DAYS = { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

    // Delivery zones configuration
    private static readonly string[] SUPPORTED_CITIES = { "Lima", "Callao", "Arequipa", "Trujillo", "Cusco" };
    private static readonly string[] PREMIUM_ZONES = { "Lima", "Callao" }; // Zones with special delivery options

    public OrderTimeAndRegionValidator()
    {
        RuleFor(x => x.RequestedDeliveryDate)
            .Must(BeWithinBusinessHours).WithMessage("Orders can only be placed during business hours (8:00 AM - 6:00 PM)")
            .Must(BeOnBusinessDay).WithMessage("Orders can only be placed Monday through Friday")
            .Must(BeValidDeliveryDate).WithMessage("Delivery date must be at least 24 hours in advance and within 30 days");

        RuleFor(x => x.ShippingAddress.City)
            .Must(BeInSupportedDeliveryZone).WithMessage($"Delivery is only available to: {string.Join(", ", SUPPORTED_CITIES)}")
            .Must(city => !string.IsNullOrWhiteSpace(city)).WithMessage("City is required for delivery validation");

        RuleFor(x => x.ShippingAddress.Region)
            .NotEmpty().WithMessage("Region/State is required for shipping calculation");

        RuleFor(x => x)
            .Must(ValidateDeliveryTimeSlot).WithMessage("Selected delivery time slot is not available for your region")
            .Must(ValidateExpressDeliveryEligibility).WithMessage("Express delivery is not available for your location or order type");
    }

    private bool BeWithinBusinessHours(DateTime? requestedDate)
    {
        if (!requestedDate.HasValue) return true; // Optional field validation

        var requestTime = requestedDate.Value.TimeOfDay;
        return requestTime >= BUSINESS_START && requestTime <= BUSINESS_END;
    }

    private bool BeOnBusinessDay(DateTime? requestedDate)
    {
        if (!requestedDate.HasValue) return true; // Optional field validation

        return BUSINESS_DAYS.Contains(requestedDate.Value.DayOfWeek);
    }

    private bool BeValidDeliveryDate(DateTime? requestedDate)
    {
        if (!requestedDate.HasValue) return true; // Optional field validation

        var now = DateTime.Now;
        var minDate = now.AddDays(1); // At least 24 hours in advance
        var maxDate = now.AddDays(30); // Within 30 days

        return requestedDate >= minDate && requestedDate <= maxDate;
    }

    private bool BeInSupportedDeliveryZone(string city)
    {
        if (string.IsNullOrWhiteSpace(city)) return false;

        return SUPPORTED_CITIES.Contains(city, StringComparer.OrdinalIgnoreCase);
    }

    private bool ValidateDeliveryTimeSlot(CreateOrderRequest request)
    {
        // If no specific delivery time is requested, it's valid
        if (!request.RequestedDeliveryDate.HasValue) return true;

        var city = request.ShippingAddress?.City;
        if (string.IsNullOrWhiteSpace(city)) return false;

        // Premium zones have more flexible delivery slots
        if (PREMIUM_ZONES.Contains(city, StringComparer.OrdinalIgnoreCase))
        {
            // Premium zones: 8 AM - 8 PM delivery slots
            var deliveryTime = request.RequestedDeliveryDate.Value.TimeOfDay;
            return deliveryTime >= new TimeSpan(8, 0, 0) && deliveryTime <= new TimeSpan(20, 0, 0);
        }
        else
        {
            // Standard zones: 9 AM - 5 PM delivery slots
            var deliveryTime = request.RequestedDeliveryDate.Value.TimeOfDay;
            return deliveryTime >= new TimeSpan(9, 0, 0) && deliveryTime <= new TimeSpan(17, 0, 0);
        }
    }

    private bool ValidateExpressDeliveryEligibility(CreateOrderRequest request)
    {
        // Express delivery validation logic
        var city = request.ShippingAddress?.City;
        if (string.IsNullOrWhiteSpace(city)) return true;

        // Express delivery only available in premium zones
        var isExpressRequested = request.RequestedDeliveryDate.HasValue && 
                                request.RequestedDeliveryDate.Value <= DateTime.Now.AddHours(24);

        if (isExpressRequested)
        {
            return PREMIUM_ZONES.Contains(city, StringComparer.OrdinalIgnoreCase);
        }

        return true; // Not requesting express delivery
    }
}
