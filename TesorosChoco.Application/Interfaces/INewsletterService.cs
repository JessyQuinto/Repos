using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;

namespace TesorosChoco.Application.Interfaces;

public interface INewsletterService
{
    Task<GenericResponse> SubscribeAsync(NewsletterSubscriptionRequest request);
    Task<GenericResponse> UnsubscribeAsync(string email);
}
