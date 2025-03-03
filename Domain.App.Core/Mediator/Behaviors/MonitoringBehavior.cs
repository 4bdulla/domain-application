using Domain.App.Core.Mediator.Abstraction;
using Domain.App.Core.Monitoring;

using MediatR;


namespace Domain.App.Core.Mediator.Behaviors;

public class MonitoringBehavior<TRequest, TResponse> : MonitoringBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
where TRequest : IBaseRequest
{
    public MonitoringBehavior(GlobalMetricReporter metricReporter) : base(metricReporter) { }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token) =>
        await HandleInternal(next);
}