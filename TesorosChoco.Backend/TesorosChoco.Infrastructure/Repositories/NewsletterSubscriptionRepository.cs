using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Newsletter subscription repository for managing email subscriptions
/// Handles subscription creation, updates, and active subscriber management
/// </summary>
public class NewsletterSubscriptionRepository : BaseRepository<NewsletterSubscription>, INewsletterSubscriptionRepository
{
    public NewsletterSubscriptionRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Create new newsletter subscription with validation
    /// </summary>
    public new async Task<NewsletterSubscription> CreateAsync(NewsletterSubscription subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(subscription.Email))
                throw new ArgumentException("Email is required for newsletter subscription");

            // Normalize email
            subscription.Email = subscription.Email.ToLowerInvariant();

            // Check if email already exists
            var existingSubscription = await GetByEmailAsync(subscription.Email);
            if (existingSubscription != null)
            {
                // If already subscribed and active, return existing
                if (existingSubscription.IsActive)
                {
                    return existingSubscription;
                }
                
                // If exists but inactive, reactivate
                existingSubscription.IsActive = true;
                existingSubscription.SubscribedAt = DateTime.UtcNow;
                existingSubscription.UnsubscribedAt = null;
                
                return await UpdateAsync(existingSubscription);
            }

            // Set timestamps and default values
            subscription.SubscribedAt = DateTime.UtcNow;
            subscription.IsActive = true;

            await _dbSet.AddAsync(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating newsletter subscription", ex);
        }
    }

    /// <summary>
    /// Get newsletter subscription by email
    /// </summary>
    public async Task<NewsletterSubscription?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be null or empty", nameof(email));

        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(ns => ns.Email.ToLower() == email.ToLowerInvariant());
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving newsletter subscription by email {email}", ex);
        }
    }

    /// <summary>
    /// Get all active newsletter subscriptions
    /// </summary>
    public async Task<IEnumerable<NewsletterSubscription>> GetAllActiveAsync()
    {
        try
        {
            return await _dbSet
                .Where(ns => ns.IsActive)
                .OrderBy(ns => ns.Email)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving active newsletter subscriptions", ex);
        }
    }

    /// <summary>
    /// Update newsletter subscription
    /// </summary>
    public new async Task<NewsletterSubscription> UpdateAsync(NewsletterSubscription subscription)
    {
        if (subscription == null)
            throw new ArgumentNullException(nameof(subscription));

        try
        {
            // If being unsubscribed, set unsubscribed timestamp
            if (!subscription.IsActive && subscription.UnsubscribedAt == null)
            {
                subscription.UnsubscribedAt = DateTime.UtcNow;
            }
            
            // If being resubscribed, clear unsubscribed timestamp and update subscribed timestamp
            if (subscription.IsActive && subscription.UnsubscribedAt != null)
            {
                subscription.UnsubscribedAt = null;
                subscription.SubscribedAt = DateTime.UtcNow;
            }

            _dbSet.Update(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Newsletter subscription was modified by another process", ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error updating newsletter subscription", ex);
        }
    }
}
