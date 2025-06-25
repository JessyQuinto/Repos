using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;
using TesorosChoco.API.Common;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/v1/contact")]
[Produces("application/json")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IContactService contactService, ILogger<ContactController> logger)
    {
        _contactService = contactService;
        _logger = logger;
    }    /// <summary>
    /// Envía un mensaje de contacto
    /// </summary>
    /// <param name="request">Datos del mensaje de contacto</param>
    /// <returns>Confirmación del envío</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GenericResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericResponse>>> SubmitContactForm([FromBody] ContactRequest request)
    {
        try
        {
            _logger.LogInformation("Processing contact form submission from {Email}", request.Email);
            
            var result = await _contactService.SendContactMessageAsync(request);
            
            _logger.LogInformation("Contact form submitted successfully from {Email}", request.Email);
            return Ok(ApiResponse<GenericResponse>.SuccessResponse(result, "Contact form submitted successfully"));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning("Invalid contact form submission: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing contact form submission");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Envía un mensaje de contacto (compatibilidad con documentación API)
    /// </summary>
    /// <param name="request">Datos del mensaje de contacto</param>
    /// <returns>Confirmación del envío</returns>
    [HttpPost("/api/contact")]
    [ProducesResponseType(typeof(ApiResponse<GenericResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GenericResponse>>> SubmitContactFormDocumentation([FromBody] ContactRequest request)
    {
        return await SubmitContactForm(request);
    }
}
