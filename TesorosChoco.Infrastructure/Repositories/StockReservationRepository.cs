using Microsoft.EntityFrameworkCore;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.Interfaces;
using TesorosChoco.Infrastructure.Data;

namespace TesorosChoco.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for stock reservation management
/// </summary>
public class StockReservationRepository : BaseRepository<StockReservation>, IStockReservationRepository
{
    private readonly TimeSpan _defaultReservationDuration = TimeSpan.FromMinutes(15); // 15 minutes default

    public StockReservationRepository(TesorosChocoDbContext context) : base(context)
    {
    }

    public async Task<StockReservation> CreateReservationAsync(int productId, int userId, int quantity, string? sessionId = null, TimeSpan? duration = null)
    {
        var reservation = new StockReservation
        {
            ProductId = productId,
            UserId = userId,
            Quantity = quantity,
            SessionId = sessionId,
            ExpiresAt = DateTime.UtcNow.Add(duration ?? _defaultReservationDuration),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return await CreateAsync(reservation);
    }

    public async Task<bool> ReleaseReservationAsync(int reservationId)
    {
        var reservation = await GetByIdAsync(reservationId);
        if (reservation == null || !reservation.IsActive)
            return false;

        reservation.Expire();
        await UpdateAsync(reservation);
        return true;
    }

    public async Task<bool> ReleaseExpiredReservationsAsync()
    {
        var expiredReservations = await _context.Set<StockReservation>()
            .Where(r => r.IsActive && DateTime.UtcNow > r.ExpiresAt)
            .ToListAsync();

        foreach (var reservation in expiredReservations)
        {
            reservation.Expire();
        }

        await _context.SaveChangesAsync();
        return expiredReservations.Any();
    }

    public async Task<bool> ReleaseUserReservationsAsync(int userId)
    {
        var userReservations = await _context.Set<StockReservation>()
            .Where(r => r.UserId == userId && r.IsActive)
            .ToListAsync();

        foreach (var reservation in userReservations)
        {
            reservation.Expire();
        }

        await _context.SaveChangesAsync();
        return userReservations.Any();
    }

    public async Task<int> GetAvailableStockAsync(int productId)
    {
        var product = await _context.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == productId);

        if (product == null)
            return 0;

        var reservedQuantity = await _context.Set<StockReservation>()
            .Where(r => r.ProductId == productId && r.IsActive && DateTime.UtcNow <= r.ExpiresAt)
            .SumAsync(r => r.Quantity);

        return Math.Max(0, product.Stock - reservedQuantity);
    }

    public async Task<IEnumerable<StockReservation>> GetActiveReservationsAsync(int productId)
    {
        return await _context.Set<StockReservation>()
            .Where(r => r.ProductId == productId && r.IsActive && DateTime.UtcNow <= r.ExpiresAt)
            .Include(r => r.Product)
            .Include(r => r.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<StockReservation>> GetUserReservationsAsync(int userId)
    {
        return await _context.Set<StockReservation>()
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.Product)
            .ToListAsync();
    }

    public async Task<bool> HasActiveReservationAsync(int productId, int userId)
    {
        return await _context.Set<StockReservation>()
            .AnyAsync(r => r.ProductId == productId && r.UserId == userId && r.IsActive && DateTime.UtcNow <= r.ExpiresAt);
    }
}
