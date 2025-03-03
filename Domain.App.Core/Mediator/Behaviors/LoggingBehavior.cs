using Domain.App.Core.Mediator.Abstraction;

using MediatR;

using Microsoft.Extensions.Logging;


namespace Domain.App.Core.Mediator.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : LoggingBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
where TRequest : IBaseRequest
{
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : base(logger) { }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token) =>
        await HandleInternal(request, next);
}