using TesorosChoco.Application.DTOs;

namespace TesorosChoco.Application.Interfaces;

public interface IProducerService
{
    Task<IEnumerable<ProducerDto>> GetAllProducersAsync();
    Task<ProducerDto?> GetProducerByIdAsync(int id);
    Task<IEnumerable<ProducerDto>> GetFeaturedProducersAsync();
}
