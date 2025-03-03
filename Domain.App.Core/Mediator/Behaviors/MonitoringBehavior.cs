using Domain.App.Core.Exceptions.Abstraction;
using Domain.App.Core.Monitoring;

using MediatR;

using Prometheus;

using ITimer = Prometheus.ITimer;


namespace Domain.App.Core.Mediator.Behaviors;

public class MonitoringBehavior<TRequest, TResponse>(DefaultMetricReporter metricReporter) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token)
    {
        TResponse result;
        string requestName = typeof(TRequest).Name;

        using ITimer timer = metricReporter.RequestProcessingDuration.WithLabels(requestName).NewTimer();
        using IDisposable progress = metricReporter.RequestInProcess.WithLabels(requestName).TrackInProgress();

        metricReporter.Requests.WithLabels(requestName).Inc();

        try
        {
            result = await next();
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case IApplicationException appException:
                    metricReporter.Exceptions.WithLabels(requestName, appException.ErrorCode).Inc();

                    break;

                default:
                    metricReporter.Exceptions.WithLabels(requestName, "other").Inc();

                    break;
            }

            throw;
        }

        return result;
    }
}