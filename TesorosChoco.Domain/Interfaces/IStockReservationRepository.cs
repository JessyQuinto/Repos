using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Repository interface for stock reservation management
/// </summary>
public interface IStockReservationRepository
{
    Task<StockReservation> CreateReservationAsync(int productId, int userId, int quantity, string? sessionId = null, TimeSpan? duration = null);
    Task<bool> ReleaseReservationAsync(int reservationId);
    Task<bool> ReleaseExpiredReservationsAsync();
    Task<bool> ReleaseUserReservationsAsync(int userId);
    Task<int> GetAvailableStockAsync(int productId);
    Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(int productId);
    Task<IEnumerable<StockReservation>> GetUserReservationsAsync(int userId);
    Task<bool> HasActiveReservationAsync(int productId, int userId);
}
