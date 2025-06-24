using TesorosChoco.Domain.Interfaces;

namespace TesorosChoco.Infrastructure.Services;

/// <summary>
/// Inventory service implementation with stock reservation capabilities
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IStockReservationRepository _stockReservationRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InventoryService(
        IStockReservationRepository stockReservationRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _stockReservationRepository = stockReservationRepository ?? throw new ArgumentNullException(nameof(stockReservationRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<bool> ReserveStockAsync(int productId, int userId, int quantity, string? sessionId = null)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Check if user already has an active reservation for this product
            if (await _stockReservationRepository.HasActiveReservationAsync(productId, userId))
            {
                return false; // User already has a reservation
            }

            // Check available stock
            var availableStock = await _stockReservationRepository.GetAvailableStockAsync(productId);
            if (availableStock < quantity)
            {
                return false; // Insufficient stock
            }

            // Create reservation
            await _stockReservationRepository.CreateReservationAsync(productId, userId, quantity, sessionId);
            return true;
        });
    }

    public async Task<bool> ReleaseReservationAsync(int productId, int userId)
    {
        var userReservations = await _stockReservationRepository.GetUserReservationsAsync(userId);
        var reservation = userReservations.FirstOrDefault(r => r.ProductId == productId);
        
        if (reservation == null)
            return false;

        return await _stockReservationRepository.ReleaseReservationAsync(reservation.Id);
    }

    public async Task<int> GetAvailableStockAsync(int productId)
    {
        return await _stockReservationRepository.GetAvailableStockAsync(productId);
    }

    public async Task<bool> ConfirmReservationAsync(int productId, int userId, int quantity)
    {
        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // Release the reservation
            await ReleaseReservationAsync(productId, userId);

            // Reduce actual stock
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null || product.Stock < quantity)
            {
                throw new InvalidOperationException($"Cannot confirm reservation: insufficient stock for product {productId}");
            }

            product.Stock -= quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product);

            return true;
        });
    }

    public async Task CleanupExpiredReservationsAsync()
    {
        await _stockReservationRepository.ReleaseExpiredReservationsAsync();
    }
}
