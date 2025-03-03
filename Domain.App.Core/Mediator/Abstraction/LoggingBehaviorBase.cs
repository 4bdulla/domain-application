using System.Diagnostics;

using MediatR;

using Microsoft.Extensions.Logging;


namespace Domain.App.Core.Mediator.Abstraction;

public abstract class LoggingBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehaviorBase<TRequest, TResponse>> _logger;

    protected LoggingBehaviorBase(ILogger<LoggingBehaviorBase<TRequest, TResponse>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    protected async Task<TResponse> HandleInternal(TRequest request, RequestHandlerDelegate<TResponse> next)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("request received: {@Request}", request);

        var result = default(TResponse);

        string requestName = typeof(TRequest).Name;

        try
        {
            result = await next();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Request} failed in {Elapsed} time due to {Message}", requestName, sw.Elapsed, ex.Message);

            throw;
        }
        finally
        {
            _logger.LogInformation("{Request} processed in {Elapsed} time with response {@Response}", requestName, sw.Elapsed, result);
        }

        return result;
    }
}