using Microsoft.AspNetCore.Mvc;

namespace TesorosChoco.API.Controllers;

[ApiController]
[Route("api/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns>API status</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = "OK",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            message = "TesorosChoco API is running"
        });
    }
}
