using MediatR;
using AutoMapper;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace TesorosChoco.Application.Handlers.Products;

public class SearchProductsQueryHandler : IRequestHandler<SearchProductsQuery, SearchProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SearchProductsQueryHandler> _logger;

    public SearchProductsQueryHandler(
        IProductRepository productRepository,
        IMapper mapper,
        ILogger<SearchProductsQueryHandler> logger)
    {
        _productRepository = productRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<SearchProductsResponse> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching products with filters: SearchTerm={SearchTerm}, CategoryId={CategoryId}, MinPrice={MinPrice}, MaxPrice={MaxPrice}, ProducerId={ProducerId}, Featured={Featured}, Limit={Limit}, Offset={Offset}",
            request.SearchTerm, request.CategoryId, request.MinPrice, request.MaxPrice, request.ProducerId, request.Featured, request.Limit, request.Offset);

        var (products, total) = await _productRepository.SearchProductsAsync(
            request.SearchTerm,
            request.CategoryId,
            request.MinPrice,
            request.MaxPrice,
            request.ProducerId,
            request.Featured,
            request.Limit,
            request.Offset);

        var productDtos = _mapper.Map<IEnumerable<Domain.Entities.Product>, IEnumerable<Application.DTOs.ProductDto>>(products);

        _logger.LogInformation("Found {Total} products matching search criteria", total);        return new SearchProductsResponse
        {
            Products = productDtos,
            Total = total,
            Page = (request.Offset / request.Limit) + 1,
            Limit = request.Limit
        };
    }
}
