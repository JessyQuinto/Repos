using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/newsletter")]
[Produces("application/json")]
public class NewsletterController : ControllerBase
{
    private readonly INewsletterService _newsletterService;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(INewsletterService newsletterService, ILogger<NewsletterController> logger)
    {
        _newsletterService = newsletterService;
        _logger = logger;
    }    /// <summary>
    /// Suscribe un email al newsletter
    /// </summary>
    /// <param name="request">Email a suscribir</param>
    /// <returns>Confirmación de suscripción</returns>
    [HttpPost("subscribe")]
    [ProducesResponseType(typeof(ApiResponse<GenericResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericResponse>>> Subscribe([FromBody] NewsletterSubscriptionRequest request)
    {
        try
        {
            _logger.LogInformation("Processing newsletter subscription for {Email}", request.Email);
            
            var result = await _newsletterService.SubscribeAsync(request);
            
            _logger.LogInformation("Newsletter subscription processed for {Email}", request.Email);
            return Ok(ApiResponse<GenericResponse>.SuccessResponse(result, "Newsletter subscription successful"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid newsletter subscription: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Newsletter subscription already exists: {Message}", ex.Message);
            return Conflict(ApiResponse.ErrorResponse(ex.Message));
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing newsletter subscription");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing your subscription"));
        }
    }

    /// <summary>
    /// Suscribe un email al newsletter (compatibilidad con documentación API)
    /// </summary>
    /// <param name="request">Email a suscribir</param>
    /// <returns>Confirmación de suscripción</returns>
    [HttpPost("/api/newsletter/subscribe")]
    [ProducesResponseType(typeof(ApiResponse<GenericResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericResponse>>> SubscribeDocumentation([FromBody] NewsletterSubscriptionRequest request)
    {
        return await Subscribe(request);
    }

    /// <summary>
    /// Cancela la suscripción al newsletter
    /// </summary>
    /// <param name="email">Email a desuscribir</param>
    /// <returns>Confirmación de cancelación</returns>
    [HttpDelete("unsubscribe/{email}")]
    [ProducesResponseType(typeof(ApiResponse<GenericResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericResponse>>> Unsubscribe(string email)
    {
        try
        {
            _logger.LogInformation("Processing newsletter unsubscription for {Email}", email);
            
            var result = await _newsletterService.UnsubscribeAsync(email);
            if (!result.Success)
            {
                return NotFound(ApiResponse<GenericResponse>.ErrorResponse("Email subscription not found", result));
            }
            
            _logger.LogInformation("Newsletter unsubscription processed for {Email}", email);
            return Ok(ApiResponse<GenericResponse>.SuccessResponse(result, "Newsletter unsubscription successful"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing newsletter unsubscription");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing your unsubscription"));
        }
    }
}
