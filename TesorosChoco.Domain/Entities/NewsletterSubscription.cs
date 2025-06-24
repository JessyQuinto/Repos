namespace TesorosChoco.Domain.Entities;

public class NewsletterSubscription : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true;
    
    public DateTime SubscribedAt { get; set; }
    
    public DateTime? UnsubscribedAt { get; set; }
}
