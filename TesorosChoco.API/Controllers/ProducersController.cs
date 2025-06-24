using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/producers")]
[Produces("application/json")]
public class ProducersController : ControllerBase
{
    private readonly IProducerService _producerService;
    private readonly IProductService _productService;
    private readonly ILogger<ProducersController> _logger;

    public ProducersController(
        IProducerService producerService, 
        IProductService productService,
        ILogger<ProducersController> logger)
    {
        _producerService = producerService;
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los productores/artesanos
    /// </summary>
    /// <returns>Lista de productores</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProducerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProducerDto>>> GetProducers()
    {
        try
        {
            _logger.LogInformation("Getting all producers");
            
            var producers = await _producerService.GetAllProducersAsync();
            
            _logger.LogInformation("Retrieved {Count} producers", producers.Count());
            return Ok(producers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting producers");
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting producers" });
        }
    }

    /// <summary>
    /// Obtiene un productor específico por su ID
    /// </summary>
    /// <param name="id">ID del productor</param>
    /// <returns>Productor específico</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProducerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProducerDto>> GetProducer(int id)
    {
        try
        {
            _logger.LogInformation("Getting producer with ID: {ProducerId}", id);
            
            var producer = await _producerService.GetProducerByIdAsync(id);
            if (producer == null)
            {
                _logger.LogWarning("Producer with ID {ProducerId} not found", id);
                return NotFound(new { error = "Producer not found", message = $"Producer with ID {id} was not found" });
            }
            
            return Ok(producer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting producer with ID {ProducerId}", id);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting the producer" });
        }
    }

    /// <summary>
    /// Obtiene productos de un productor específico
    /// </summary>
    /// <param name="producerId">ID del productor</param>
    /// <returns>Lista de productos del productor</returns>
    [HttpGet("{producerId}/products")]
    [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByProducer(int producerId)
    {
        try
        {
            _logger.LogInformation("Getting products for producer: {ProducerId}", producerId);
            
            // First verify the producer exists
            var producer = await _producerService.GetProducerByIdAsync(producerId);
            if (producer == null)
            {
                _logger.LogWarning("Producer with ID {ProducerId} not found", producerId);
                return NotFound(new { error = "Producer not found", message = $"Producer with ID {producerId} was not found" });
            }
            
            var products = await _productService.GetProductsByProducerAsync(producerId);
            
            _logger.LogInformation("Retrieved {Count} products for producer {ProducerId}", products.Count(), producerId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for producer {ProducerId}", producerId);
            return StatusCode(500, new { error = "Internal server error", message = "An error occurred while getting products for the producer" });
        }    }
}
