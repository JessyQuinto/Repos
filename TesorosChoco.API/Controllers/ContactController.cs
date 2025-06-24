using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }

    /// <summary>
    /// Envía un mensaje de contacto
    /// </summary>
    /// <param name="request">Datos del mensaje de contacto</param>
    /// <returns>Confirmación del envío</returns>
    [HttpPost]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> SubmitContactForm([FromBody] ContactRequest request)
    {
        try
        {
            _logger.LogInformation("Processing contact form submission from {Email}", request.Email);
            
            var result = await _contactService.SendContactMessageAsync(request);
            
            _logger.LogInformation("Contact form submitted successfully from {Email}", request.Email);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid contact form submission: {Message}", ex.Message);
            return BadRequest(new GenericResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            return StatusCode(500, new GenericResponse
            {
                Success = false,
                Message = "An error occurred while processing your request"
            });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(INewsletterService newsletterService, ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }

    /// <summary>
    /// Suscribe un email al newsletter
    /// </summary>
    /// <param name="request">Email a suscribir</param>
    /// <returns>Confirmación de suscripción</returns>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> Subscribe([FromBody] NewsletterSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Processing newsletter subscription for {Email}", request.Email);
            
            var result = await _newsletterService.SubscribeAsync(request);
            
            _logger.LogInformation("Newsletter subscription processed for {Email}", request.Email);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid newsletter subscription: {Message}", ex.Message);
            return BadRequest(new GenericResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Newsletter subscription already exists: {Message}", ex.Message);
            return Conflict(new GenericResponse
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing newsletter subscription");
            return StatusCode(500, new GenericResponse
            {
                Success = false,
                Message = "An error occurred while processing your subscription"
            });
        }
    }

    /// <summary>
    /// Cancela la suscripción al newsletter
    /// </summary>
    /// <param name="email">Email a desuscribir</param>
    /// <returns>Confirmación de cancelación</returns>
    [HttpDelete("unsubscribe/{email}")]
    [ProducesResponseType(typeof(GenericResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponse>> Unsubscribe(string email)
    {
        try
        {
            _logger.LogInformation("Processing newsletter unsubscription for {Email}", email);
            
            var result = await _newsletterService.UnsubscribeAsync(email);
            if (!result.Success)
            {
                return NotFound(result);
            }
            
            _logger.LogInformation("Newsletter unsubscription processed for {Email}", email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing newsletter unsubscription");
            return StatusCode(500, new GenericResponse
            {
                Success = false,
                Message = "An error occurred while processing your unsubscription"
            });
        }
    }
}
