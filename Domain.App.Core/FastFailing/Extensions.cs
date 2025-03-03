using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;


namespace Domain.App.Core.FastFailing;

public static class Extensions
{
    /// <summary>
    ///     Adds <see cref="FailFastTask" /> to the IoC container
    /// </summary>
    /// <param name="builder">
    ///     <see cref="WebApplicationBuilder" />
    /// </param>
    public static void AddFastFailing(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<FailFastTask>().TryAddSingleton(builder.Services);
    }

    /// <summary>
    ///     Runs Execute method of <see cref="FailFastTask" />
    /// </summary>
    /// <param name="app">
    ///     <see cref="WebApplication" />
    /// </param>
    /// <exception cref="InvalidOperationException">
    ///     In case if <see cref="FailFastTask" /> is not added to <see cref="IServiceCollection" />
    /// </exception>
    public static void RunWithFastFailing(this WebApplication app)
    {
        FailFastTask failFastTask = app.Services.GetService<FailFastTask>() ??
            throw new InvalidOperationException("FailFastTask has not been registered.");

        failFastTask.Execute();

        app.Run();
    }
}