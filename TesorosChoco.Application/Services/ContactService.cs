using AutoMapper;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Contact service implementation for handling contact messages
/// Stores contact messages and provides basic validation
/// </summary>
public class ContactService : IContactService
{
    private readonly IContactMessageRepository _contactMessageRepository;
    private readonly IMapper _mapper;

    public ContactService(IContactMessageRepository contactMessageRepository, IMapper mapper)
    {
        _contactMessageRepository = contactMessageRepository ?? throw new ArgumentNullException(nameof(contactMessageRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GenericResponse> SendContactMessageAsync(ContactRequest request)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            if (string.IsNullOrWhiteSpace(request.Message))
                throw new ArgumentException("Message is required");

            // Create contact message entity
            var contactMessage = new ContactMessage
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim().ToLowerInvariant(),
                Subject = request.Subject?.Trim() ?? string.Empty,
                Message = request.Message.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _contactMessageRepository.CreateAsync(contactMessage);

            return new GenericResponse
            {
                Success = true,
                Message = "Your message has been sent successfully. We'll get back to you soon!"
            };
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = $"Error sending message: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Newsletter service implementation for managing subscriptions
/// Handles subscription creation and unsubscription
/// </summary>
public class NewsletterService : INewsletterService
{
    private readonly INewsletterSubscriptionRepository _newsletterRepository;
    private readonly IMapper _mapper;

    public NewsletterService(INewsletterSubscriptionRepository newsletterRepository, IMapper mapper)
    {
        _newsletterRepository = newsletterRepository ?? throw new ArgumentNullException(nameof(newsletterRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GenericResponse> SubscribeAsync(NewsletterSubscriptionRequest request)
    {
        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required");

            var email = request.Email.Trim().ToLowerInvariant();

            // Check if email is already subscribed
            var existingSubscription = await _newsletterRepository.GetByEmailAsync(email);
            
            if (existingSubscription != null)
            {
                if (existingSubscription.IsActive)
                {
                    return new GenericResponse
                    {
                        Success = true,
                        Message = "You are already subscribed to our newsletter!"
                    };
                }
                else
                {
                    // Reactivate subscription
                    existingSubscription.IsActive = true;
                    existingSubscription.UpdatedAt = DateTime.UtcNow;
                    await _newsletterRepository.UpdateAsync(existingSubscription);

                    return new GenericResponse
                    {
                        Success = true,
                        Message = "Welcome back! Your newsletter subscription has been reactivated."
                    };
                }
            }

            // Create new subscription
            var subscription = new NewsletterSubscription
            {
                Email = email,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _newsletterRepository.CreateAsync(subscription);

            return new GenericResponse
            {
                Success = true,
                Message = "Thank you for subscribing to our newsletter!"
            };
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = $"Error subscribing to newsletter: {ex.Message}"
            };
        }
    }

    public async Task<GenericResponse> UnsubscribeAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required");

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var subscription = await _newsletterRepository.GetByEmailAsync(normalizedEmail);

            if (subscription == null || !subscription.IsActive)
            {
                return new GenericResponse
                {
                    Success = true,
                    Message = "Email not found in our newsletter subscription list."
                };
            }

            // Deactivate subscription
            subscription.IsActive = false;
            subscription.UpdatedAt = DateTime.UtcNow;
            await _newsletterRepository.UpdateAsync(subscription);

            return new GenericResponse
            {
                Success = true,
                Message = "You have been successfully unsubscribed from our newsletter."
            };
        }
        catch (Exception ex)
        {
            return new GenericResponse
            {
                Success = false,
                Message = $"Error unsubscribing from newsletter: {ex.Message}"
            };
        }
    }
}
