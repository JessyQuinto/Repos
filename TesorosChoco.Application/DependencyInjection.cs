using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using MediatR;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Application.Services;
using TesorosChoco.Application.Mappings;

namespace TesorosChoco.Application;

/// <summary>
/// Application layer dependency injection configuration
/// Registers all application services, validators, and MediatR handlers
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile).Assembly);

        // MediatR for CQRS pattern
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());        // Application Services
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
