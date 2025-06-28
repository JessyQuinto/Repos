namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Cache service interface for improving performance
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get cached value by key
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Set cached value with expiration
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    
    /// <summary>
    /// Remove cached value
    /// </summary>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Remove cached values by pattern
    /// </summary>
    Task RemoveByPatternAsync(string pattern);
    
    /// <summary>
    /// Check if key exists in cache
    /// </summary>
    Task<bool> ExistsAsync(string key);
    
    /// <summary>
    /// Clear all cache
    /// </summary>
    Task ClearAsync();
}
