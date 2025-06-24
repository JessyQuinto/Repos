namespace TesorosChoco.Domain.Interfaces;

/// <summary>
/// Unit of Work pattern interface for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begins a new transaction
    /// </summary>
    Task BeginTransactionAsync();
    
    /// <summary>
    /// Commits the current transaction
    /// </summary>
    Task CommitTransactionAsync();
    
    /// <summary>
    /// Rolls back the current transaction
    /// </summary>
    Task RollbackTransactionAsync();
    
    /// <summary>
    /// Saves all changes within the current transaction
    /// </summary>
    Task<int> SaveChangesAsync();
    
    /// <summary>
    /// Executes an action within a transaction, automatically committing or rolling back
    /// </summary>
    Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action);
    
    /// <summary>
    /// Executes an action within a transaction, automatically committing or rolling back
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> action);
}
