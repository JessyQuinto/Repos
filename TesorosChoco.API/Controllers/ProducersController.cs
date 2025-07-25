using Microsoft.AspNetCore.Mvc;
using MediatR;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.Application.Queries.Products;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/producers")]
[Produces("application/json")]
public class ProducersController : BaseController
{
    private readonly IProducerService _producerService;
    private readonly IMediator _mediator;
    private readonly ILogger<ProducersController> _logger;

    public ProducersController(
        IProducerService producerService,
        IMediator mediator,
        ILogger<ProducersController> logger)
    {
        _producerService = producerService ?? throw new ArgumentNullException(nameof(producerService));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }    /// <summary>
    /// Obtiene todos los productores/artesanos
    /// </summary>
    /// <returns>Lista de productores</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProducerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProducerDto>>>> GetProducers()
    {
        try
        {
            _logger.LogInformation("Getting all producers");
            
            var producers = await _producerService.GetAllProducersAsync();
            
            _logger.LogInformation("Retrieved {Count} producers", producers.Count());
            return Ok(ApiResponse<IEnumerable<ProducerDto>>.SuccessResponse(producers, "Producers retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting producers");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting producers"));
        }
    }    /// <summary>
    /// Obtiene un productor específico por su ID
    /// </summary>
    /// <param name="id">ID del productor</param>
    /// <returns>Productor específico</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProducerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ProducerDto>>> GetProducer(int id)
    {
        try
        {
            _logger.LogInformation("Getting producer with ID: {ProducerId}", id);
            
            var producer = await _producerService.GetProducerByIdAsync(id);
            if (producer == null)
            {
                _logger.LogWarning("Producer with ID {ProducerId} not found", id);
                return NotFound(ApiResponse.ErrorResponse($"Producer with ID {id} was not found"));
            }
            
            return Ok(ApiResponse<ProducerDto>.SuccessResponse(producer, "Producer retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting producer with ID {ProducerId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting the producer"));
        }
    }

    /// <summary>
    /// Obtiene productos de un productor específico
    /// </summary>
    /// <param name="producerId">ID del productor</param>
    /// <returns>Lista de productos del productor</returns>
    [HttpGet("{producerId}/products")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetProductsByProducer(int producerId)
    {
        try
        {
            _logger.LogInformation("Getting products for producer: {ProducerId}", producerId);
            
            // First verify the producer exists
            var producer = await _producerService.GetProducerByIdAsync(producerId);
            if (producer == null)
            {
                _logger.LogWarning("Producer with ID {ProducerId} not found", producerId);
                return NotFound(ApiResponse.ErrorResponse($"Producer with ID {producerId} was not found"));
            }
            
            var query = new GetAllProductsQuery { ProducerId = producerId };
            var products = await _mediator.Send(query);
            
            _logger.LogInformation("Retrieved {Count} products for producer {ProducerId}", products.Count(), producerId);
            return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(products, "Products retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for producer {ProducerId}", producerId);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while getting products for the producer"));
        }
    }
}
