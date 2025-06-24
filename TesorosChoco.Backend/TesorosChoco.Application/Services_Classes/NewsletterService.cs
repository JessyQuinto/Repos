using AutoMapper;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

public class NewsletterService : INewsletterService
{
    private readonly INewsletterSubscriptionRepository _newsletterRepository;
    private readonly IMapper _mapper;

    public NewsletterService(INewsletterSubscriptionRepository newsletterRepository, IMapper mapper)
    {
        _newsletterRepository = newsletterRepository;
        _mapper = mapper;
    }

    public async Task<GenericResponse> SubscribeToNewsletterAsync(NewsletterSubscriptionRequest request)
    {
        try
        {
            // Check if email is already subscribed
            var existingSubscription = await _newsletterRepository.GetByEmailAsync(request.Email);
            if (existingSubscription != null)
            {
                return new GenericResponse
                {
                    Success = false,
                    Message = "This email is already subscribed to our newsletter."
                };
            }

            var subscription = new NewsletterSubscription
            {
                Email = request.Email,
                SubscribedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _newsletterRepository.CreateAsync(subscription);

            return new GenericResponse
            {
                Success = true,
                Message = "You have been successfully subscribed to our newsletter!"
            };
        }
        catch (Exception)
        {
            return new GenericResponse
            {
                Success = false,
                Message = "There was an error subscribing to the newsletter. Please try again later."
            };
        }
    }
}
