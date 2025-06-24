using MediatR;
using Microsoft.Extensions.Logging;
using TesorosChoco.Domain.Exceptions;

namespace TesorosChoco.Application.Behaviors;

/// <summary>
/// Pipeline behavior for centralized exception handling and transformation
/// </summary>
public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> _logger;

    public ExceptionHandlingBehavior(ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain exception in {RequestName}: {Message}", 
                typeof(TRequest).Name, ex.Message);
            throw; // Let domain exceptions bubble up as they are expected business logic violations
        }
        catch (FluentValidation.ValidationException ex)
        {
            _logger.LogWarning("Validation exception in {RequestName}: {Errors}", 
                typeof(TRequest).Name, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            throw; // Let validation exceptions bubble up
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Invalid operation in {RequestName}: {Message}", 
                typeof(TRequest).Name, ex.Message);
            throw; // Business rule violations
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Resource not found in {RequestName}: {Message}", 
                typeof(TRequest).Name, ex.Message);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access in {RequestName}: {Message}", 
                typeof(TRequest).Name, ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in {RequestName}: {Message}", 
                typeof(TRequest).Name, ex.Message);
            
            // Transform unexpected exceptions into a generic application exception
            throw new ApplicationException($"An error occurred while processing {typeof(TRequest).Name}", ex);
        }
    }
}
