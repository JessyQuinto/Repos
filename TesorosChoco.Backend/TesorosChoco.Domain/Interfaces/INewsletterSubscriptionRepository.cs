using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface INewsletterSubscriptionRepository
{
    Task<NewsletterSubscription> CreateAsync(NewsletterSubscription subscription);
    Task<NewsletterSubscription?> GetByEmailAsync(string email);
    Task<IEnumerable<NewsletterSubscription>> GetAllActiveAsync();
    Task<NewsletterSubscription> UpdateAsync(NewsletterSubscription subscription);
}
