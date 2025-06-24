using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace TesorosChoco.Application.Behaviors;

/// <summary>
/// Pipeline behavior for performance monitoring and slow query detection
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private const int SlowQueryThresholdMs = 500;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestName = typeof(TRequest).Name;

        try
        {
            var response = await next();
            stopwatch.Stop();

            var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

            if (elapsedMilliseconds > SlowQueryThresholdMs)
            {
                _logger.LogWarning("Slow query detected: {RequestName} took {ElapsedMilliseconds}ms to execute. Request: {@Request}",
                    requestName, elapsedMilliseconds, request);
            }

            _logger.LogInformation("Request {RequestName} completed in {ElapsedMilliseconds}ms",
                requestName, elapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedMilliseconds}ms", 
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
