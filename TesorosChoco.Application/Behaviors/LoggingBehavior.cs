using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TesorosChoco.Application.Behaviors;

/// <summary>
/// Pipeline behavior for logging request/response and performance monitoring
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid().ToString();

        _logger.LogInformation("Processing request {RequestName} with ID {RequestId}: {@Request}", 
            requestName, requestId, request);

        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var response = await next();
            
            stopwatch.Stop();
            _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms", 
                requestName, requestId, stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {ElapsedMilliseconds}ms", 
                requestName, requestId, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
