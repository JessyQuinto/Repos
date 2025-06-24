namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Service interface for inventory management with stock reservations
/// </summary>
public interface IInventoryService
{
    /// <summary>
    /// Reserves stock for a user during checkout process
    /// </summary>
    Task<bool> ReserveStockAsync(int productId, int userId, int quantity, string? sessionId = null);
    
    /// <summary>
    /// Releases stock reservation
    /// </summary>
    Task<bool> ReleaseReservationAsync(int productId, int userId);
    
    /// <summary>
    /// Gets available stock considering active reservations
    /// </summary>
    Task<int> GetAvailableStockAsync(int productId);
    
    /// <summary>
    /// Confirms reservation and reduces actual stock (used during order completion)
    /// </summary>
    Task<bool> ConfirmReservationAsync(int productId, int userId, int quantity);
    
    /// <summary>
    /// Cleanup expired reservations
    /// </summary>
    Task CleanupExpiredReservationsAsync();
}
