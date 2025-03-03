using System.Diagnostics;

using MediatR;

using Microsoft.Extensions.Logging;


namespace Domain.App.Core.Mediator.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();

        logger.LogInformation("request received: {@Request}", request);

        var result = default(TResponse);

        string requestName = typeof(TRequest).Name;

        try
        {
            result = await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Request} failed in {Elapsed} time due to {Message}", requestName, sw.Elapsed, ex.Message);

            throw;
        }
        finally
        {
            logger.LogInformation("{Request} processed in {Elapsed} time with response {@Response}", requestName, sw.Elapsed, result);
        }

        return result;
    }
}