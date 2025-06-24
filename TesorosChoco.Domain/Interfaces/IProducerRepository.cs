using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

public interface IProducerRepository
{
    Task<Producer?> GetByIdAsync(int id);
    Task<IEnumerable<Producer>> GetAllAsync();
    Task<IEnumerable<Producer>> GetFeaturedAsync();
    Task<Producer> CreateAsync(Producer producer);
    Task<Producer> UpdateAsync(Producer producer);
    Task DeleteAsync(int id);
}
