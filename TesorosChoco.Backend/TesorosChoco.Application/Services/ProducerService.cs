using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

/// <summary>
/// Producer service implementation with featured producer capabilities
/// Optimized for producer catalog and product associations
/// </summary>
public class ProducerService : IProducerService
{
    private readonly IProducerRepository _producerRepository;
    private readonly IMapper _mapper;

    public ProducerService(IProducerRepository producerRepository, IMapper mapper)
    {
        _producerRepository = producerRepository ?? throw new ArgumentNullException(nameof(producerRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IEnumerable<ProducerDto>> GetAllProducersAsync()
    {
        try
        {
            var producers = await _producerRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ProducerDto>>(producers);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving all producers", ex);
        }
    }

    public async Task<ProducerDto?> GetProducerByIdAsync(int id)
    {
        try
        {
            var producer = await _producerRepository.GetByIdAsync(id);
            return producer != null ? _mapper.Map<ProducerDto>(producer) : null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error retrieving producer with ID {id}", ex);
        }
    }
}
