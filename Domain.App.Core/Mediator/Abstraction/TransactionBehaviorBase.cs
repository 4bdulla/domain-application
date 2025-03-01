using System.Diagnostics;
using System.Threading;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Domain.App.Core.Mediator.Abstraction;

public abstract class TransactionBehaviorBase<TRequest, TResponse>
{
    private readonly ILogger<TransactionBehaviorBase<TRequest, TResponse>> _logger;
    private readonly DbContext _dbContext;


    protected TransactionBehaviorBase(DbContext dbContext, ILogger<TransactionBehaviorBase<TRequest, TResponse>> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    protected async Task<TResponse> HandleInternal(RequestHandlerDelegate<TResponse> next, CancellationToken token)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogDebug("starting transaction");

        IDbContextTransaction currentTx = _dbContext.Database.CurrentTransaction;

        if (currentTx is not null)
        {
            TResponse response = await next();

            _logger.LogDebug("using existing transaction {Id}", currentTx.TransactionId);

            return response;
        }

        IDbContextTransaction tx = await _dbContext.Database.BeginTransactionAsync(token);

        _logger.LogDebug("transaction {Id} started in {Elapsed} time", tx.TransactionId, sw.Elapsed);

        try
        {
            TResponse response = await next();

            _logger.LogDebug("committing transaction {Id}", tx.TransactionId);

            sw.Restart();

            await tx.CommitAsync(token);

            _logger.LogDebug("transaction {Id} committed in {Elapsed} time", tx.TransactionId, sw.Elapsed);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "rolling back transaction {Id} due to {Message}", tx.TransactionId, ex.Message);

            sw.Restart();

            await tx.RollbackAsync(token);

            _logger.LogDebug("transaction {Id} rolled back in {Elapsed} time", tx.TransactionId, sw.Elapsed);

            throw;
        }
    }
}