using Domain.App.Core.Exceptions.Abstraction;
using Domain.App.Core.Monitoring;
using MediatR;
using Prometheus;

namespace Domain.App.Core.Mediator.Abstraction;

public abstract class MonitoringBehaviorBase<TRequest, TResponse>
{
    private readonly GlobalMetricReporter _metricReporter;

    protected MonitoringBehaviorBase(GlobalMetricReporter metricReporter)
    {
        _metricReporter = metricReporter ?? throw new ArgumentNullException(nameof(metricReporter));
    }

    protected async Task<TResponse> HandleInternal(RequestHandlerDelegate<TResponse> next)
    {
        TResponse result;
        string requestName = typeof(TRequest).Name;

        using ITimer timer = _metricReporter.RequestProcessingDuration.WithLabels(requestName).NewTimer();
        using IDisposable progress = _metricReporter.RequestInProcess.WithLabels(requestName).TrackInProgress();

        _metricReporter.Requests.WithLabels(requestName).Inc();

        try
        {
            result = await next();
        }
        catch (Exception ex)
        {
            switch (ex)
            {
                case IApplicationException appException:
                    _metricReporter.Exceptions.WithLabels(requestName, appException.ErrorCode).Inc();

                    break;

                default:
                    _metricReporter.Exceptions.WithLabels(requestName, "other").Inc();

                    break;
            }

            throw;
        }

        return result;
    }
}