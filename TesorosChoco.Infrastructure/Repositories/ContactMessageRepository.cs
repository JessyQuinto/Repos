using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Contact message repository for managing customer inquiries and messages
/// Optimized for message storage and retrieval for customer service
/// </summary>
public class ContactMessageRepository : BaseRepository<ContactMessage>, IContactMessageRepository
{
    public ContactMessageRepository(TesorosChocoDbContext context) : base(context)
    {
    }    /// <summary>
    /// Create new contact message with validation and initialization
    /// </summary>
    public new async Task<ContactMessage> CreateAsync(ContactMessage contactMessage)
    {
        if (contactMessage == null)
            throw new ArgumentNullException(nameof(contactMessage));

        try
        {
            // Set creation timestamp
            contactMessage.CreatedAt = DateTime.UtcNow;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(contactMessage.Name))
                throw new ArgumentException("Contact name is required");

            if (string.IsNullOrWhiteSpace(contactMessage.Email))
                throw new ArgumentException("Contact email is required");

            if (string.IsNullOrWhiteSpace(contactMessage.Message))
                throw new ArgumentException("Contact message is required");

            // Normalize email
            contactMessage.Email = contactMessage.Email.ToLowerInvariant();

            await _dbSet.AddAsync(contactMessage);
            await _context.SaveChangesAsync();

            return contactMessage;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Error creating contact message", ex);
        }
    }    /// <summary>
    /// Get all contact messages ordered by creation date (newest first)
    /// </summary>
    public async Task<IEnumerable<ContactMessage>> GetAllAsync()
    {
        try
        {
            return await _dbSet
                .OrderByDescending(cm => cm.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all contact messages", ex);
        }
    }

    /// <summary>
    /// Get contact message by ID
    /// </summary>
    public new async Task<ContactMessage?> GetByIdAsync(int id)
    {
        try
        {
            return await _dbSet
                .FirstOrDefaultAsync(cm => cm.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving contact message by ID {id}", ex);
        }
    }
}
