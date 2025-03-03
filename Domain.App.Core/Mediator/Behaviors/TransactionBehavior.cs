using Domain.App.Core.Mediator.Abstraction;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace Domain.App.Core.Mediator.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : TransactionBehaviorBase<TRequest, TResponse>, IPipelineBehavior<TRequest, TResponse>
where TRequest : IBaseRequest
{
    public TransactionBehavior(DbContext dbContext, ILogger<TransactionBehavior<TRequest, TResponse>> logger) : base(dbContext, logger) { }


    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token) =>
        await HandleInternal(next, token);
}