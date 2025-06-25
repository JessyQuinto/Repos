using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Background service for cleaning up expired stock reservations
/// Runs periodically to maintain database hygiene and release expired reservations
/// </summary>
public class StockReservationCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockReservationCleanupService> _logger;
    private readonly TimeSpan _cleanupInterval;

    public StockReservationCleanupService(
        IServiceProvider serviceProvider,
        ILogger<StockReservationCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cleanupInterval = TimeSpan.FromMinutes(5); // Run cleanup every 5 minutes
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Reservation Cleanup Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformCleanupAsync();
                await Task.Delay(_cleanupInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during stock reservation cleanup");
                
                // Wait a shorter time before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Stock Reservation Cleanup Service stopped");
    }

    private async Task PerformCleanupAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        _logger.LogDebug("Starting stock reservation cleanup");

        try
        {
            await inventoryService.CleanupExpiredReservationsAsync();
            _logger.LogDebug("Stock reservation cleanup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired stock reservations");
            throw; // Re-throw to be handled by the outer catch block
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stock Reservation Cleanup Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
