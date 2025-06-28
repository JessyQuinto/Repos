using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TesorosChoco.Infrastructure.Data;
using System.Reflection;
using System.Diagnostics;

namespace TesorosChoco.API.Controllers;

/// <summary>
/// Health check controller for monitoring API and dependencies status
/// Provides comprehensive health information for monitoring and alerting
/// </summary>
[ApiController]
[Route("api/v1/health")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly TesorosChocoDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(TesorosChocoDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>API status</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public ActionResult GetHealth()
    {
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                version,
                application = "TesorosChoco API",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = "Health check failed"
            });
        }
    }

    /// <summary>
    /// Comprehensive health check with dependencies
    /// </summary>
    /// <returns>Detailed health status</returns>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetDetailedHealth()
    {
        var healthChecks = new Dictionary<string, object>();
        var overallHealthy = true;

        try
        {
            // Check database connectivity
            var dbHealthy = await CheckDatabaseHealthAsync();
            healthChecks["database"] = new
            {
                status = dbHealthy ? "Healthy" : "Unhealthy",
                responseTime = DateTime.UtcNow
            };
            overallHealthy &= dbHealthy;

            // Check memory usage
            var memoryUsage = GC.GetTotalMemory(false);
            healthChecks["memory"] = new
            {
                status = "Healthy",
                usedBytes = memoryUsage,
                usedMB = Math.Round(memoryUsage / 1024.0 / 1024.0, 2)
            };

            // Check application uptime
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            healthChecks["uptime"] = new
            {
                status = "Healthy",
                uptimeSeconds = (int)uptime.TotalSeconds,
                uptimeFormatted = uptime.ToString(@"dd\.hh\:mm\:ss")
            };

            var status = overallHealthy ? "Healthy" : "Degraded";
            var statusCode = overallHealthy ? 200 : 503;

            return StatusCode(statusCode, new
            {
                status,
                timestamp = DateTime.UtcNow,
                version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0",
                checks = healthChecks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = "Health check failed",
                checks = healthChecks
            });
        }
    }

    private async Task<bool> CheckDatabaseHealthAsync()
    {
        try
        {
            // Simple connectivity test
            await _dbContext.Database.CanConnectAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed");
            return false;
        }
    }
}
