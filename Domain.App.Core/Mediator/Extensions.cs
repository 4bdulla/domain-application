using Domain.App.Core.Mediator.Behaviors;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace Domain.App.Core.Mediator;

public static class Extensions
{
    /// <summary>
    ///     Executes AddMediatR method and registers handlers from executing assembly.
    ///     Adds <see cref="MonitoringBehavior{TRequest,TResponse}" /> as open behavior to all requests
    /// </summary>
    /// <param name="builder">
    ///     <see cref="WebApplicationBuilder" />
    /// </param>
    /// <param name="addTransactionBehavior">controls whether to include transaction behavior to the build (defaults to true)</param>
    /// <returns>
    ///     <see cref="IServiceCollection" />
    /// </returns>
    public static void AddMediator(this WebApplicationBuilder builder, bool addTransactionBehavior = true)
    {
        builder.Services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            configuration.AddOpenBehavior(typeof(MonitoringBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));

            if (!addTransactionBehavior)
                return;

            configuration.AddOpenBehavior(typeof(TransactionBehavior<,>));
        });
    }
}