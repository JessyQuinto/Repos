using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;

namespace TesorosChoco.Application.Interfaces;

public interface IContactService
{
    Task<GenericResponse> SubmitContactFormAsync(ContactRequest request);
}

public interface INewsletterService
{
    Task<GenericResponse> SubscribeAsync(NewsletterSubscriptionRequest request);
    Task<GenericResponse> UnsubscribeAsync(string email);
}
