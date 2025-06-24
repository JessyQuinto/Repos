using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Application.Mappings;

/// <summary>
/// AutoMapper profile for entity-DTO mappings
/// Defines mapping configurations for all domain entities and DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Will be set separately
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service

        CreateMap<UpdateProfileRequest, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Email, opt => opt.Ignore()) // Email should not be updated via profile
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service

        // Product mappings
        CreateMap<Product, ProductDto>();
        CreateMap<CreateProductRequest, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Producer, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service

        CreateMap<UpdateProductRequest, Product>()
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Producer, opt => opt.Ignore())
            .ForMember(dest => dest.CartItems, opt => opt.Ignore())
            .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service

        // Category mappings
        CreateMap<Category, CategoryDto>();

        // Producer mappings
        CreateMap<Producer, ProducerDto>();        // Cart mappings
        CreateMap<Cart, CartDto>()
            .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(item => item.Quantity * item.Price)));

        CreateMap<CartItem, CartItemDto>();

        // Order mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<OrderItem, OrderItemDto>();

        // Contact mappings
        CreateMap<ContactRequest, ContactMessage>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service

        // Newsletter mappings
        CreateMap<NewsletterSubscriptionRequest, NewsletterSubscription>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // Will be set in service
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore()); // Will be set in service
    }
}