using Microsoft.AspNetCore.Mvc;
using TesorosChoco.Application.DTOs.Requests;
using TesorosChoco.Application.DTOs.Responses;
using TesorosChoco.Application.Interfaces;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/contact")]
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
        }    }
}
