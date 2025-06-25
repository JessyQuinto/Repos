using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Application.Services;
using TesorosChoco.Application.Mappings;
using TesorosChoco.Application.Behaviors;
using TesorosChoco.Application.Validators;

namespace TesorosChoco.Application;

/// <summary>
/// Application layer dependency injection configuration
/// Registers all application services, validators, MediatR handlers, and pipeline behaviors
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);        // MediatR for CQRS pattern with pipeline behaviors (order matters!)
        services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ExceptionHandlingBehavior<,>));
        });        // FluentValidation
        services.AddValidatorsFromAssembly(assembly);        // Business Rules Validators
        services.AddScoped<CreateOrderBusinessRulesValidator>();
        services.AddScoped<TesorosChoco.Application.Validators.Orders.OrderValueAndLimitsValidator>();
        services.AddScoped<TesorosChoco.Application.Validators.Orders.OrderTimeAndRegionValidator>();
        services.AddScoped<TesorosChoco.Application.Validators.Products.ProductBusinessRulesValidator>();

        // Application Services (legacy - will be gradually replaced by CQRS)
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProducerService, ProducerService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<INewsletterService, NewsletterService>();

        return services;
    }
}
