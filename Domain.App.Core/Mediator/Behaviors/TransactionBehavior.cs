using System.Diagnostics;

using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;


namespace Domain.App.Core.Mediator.Behaviors;

public class TransactionBehavior<TRequest, TResponse>(
    DbContext dbContext,
    ILogger<TransactionBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();

        logger.LogDebug("starting transaction");

        IDbContextTransaction currentTx = dbContext.Database.CurrentTransaction;

        if (currentTx is not null)
        {
            TResponse response = await next();

            logger.LogDebug("using existing transaction {Id}", currentTx.TransactionId);

            return response;
        }

        IDbContextTransaction tx = await dbContext.Database.BeginTransactionAsync(token);

        logger.LogDebug("transaction {Id} started in {Elapsed} time", tx.TransactionId, sw.Elapsed);

        try
        {
            TResponse response = await next();

            logger.LogDebug("committing transaction {Id}", tx.TransactionId);

            sw.Restart();

            await tx.CommitAsync(token);

            logger.LogDebug("transaction {Id} committed in {Elapsed} time", tx.TransactionId, sw.Elapsed);

            return response;
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "rolling back transaction {Id} due to {Message}", tx.TransactionId, ex.Message);

            sw.Restart();

            await tx.RollbackAsync(token);

            logger.LogDebug("transaction {Id} rolled back in {Elapsed} time", tx.TransactionId, sw.Elapsed);

            throw;
        }
    }
}