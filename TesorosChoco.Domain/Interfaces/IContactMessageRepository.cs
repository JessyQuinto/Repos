using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface IContactMessageRepository
{
    Task<ContactMessage> CreateAsync(ContactMessage contactMessage);
    Task<IEnumerable<ContactMessage>> GetAllAsync();
    Task<ContactMessage?> GetByIdAsync(int id);
}
