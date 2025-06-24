using AutoMapper;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Application.Services;

public class ProducerService : IProducerService
{
    private readonly IProducerRepository _producerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public ProducerService(IProducerRepository producerRepository, IProductRepository productRepository, IMapper mapper)
    {
        _producerRepository = producerRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProducerDto>> GetAllProducersAsync()
    {
        var producers = await _producerRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProducerDto>>(producers);
    }

    public async Task<ProducerDto?> GetProducerByIdAsync(int producerId)
    {
        var producer = await _producerRepository.GetByIdAsync(producerId);
        return producer != null ? _mapper.Map<ProducerDto>(producer) : null;
    }

    public async Task<IEnumerable<ProductDto>> GetProductsByProducerAsync(int producerId)
    {
        var products = await _productRepository.GetByProducerIdAsync(producerId);
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<IEnumerable<ProducerDto>> GetFeaturedProducersAsync()
    {
        var producers = await _producerRepository.GetFeaturedAsync();
        return _mapper.Map<IEnumerable<ProducerDto>>(producers);
    }
}
