using System.Text.Json;
using Domain.App.Core.Integration;
using Domain.App.Core.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Domain.App.Core;

public static class DomainApplication
{
    public static async Task RunAsync(
        string[] args,
        Action<WebApplicationBuilder> configureServices = null,
        Action<WebApplication> configureApplication = null,
        Func<JsonSerializerOptions> serializerOptions = null) =>
        await DomainApplication.InternalRunAsync(args,
            wrapper =>
            {
                wrapper
                    .ConfigureApplicationDefaults(false, serializerOptions?.Invoke() ?? JsonHandling.Options)
                    .ConfigureApplicationAuthorizationOptions()
                    .ConfigureApplicationAuthorizationClient()
                    .ConfigureServices(configureServices)
                    .ConfigureSwagger()
                    .BuildDomainApplication()
                    .UseApplicationDefaults()
                    .UseApplicationApiEndpoints()
                    .UseApplicationConfiguration(configureApplication)
                    .Run();

                return Task.CompletedTask;
            });

    public static async Task RunWithDbAsync<TDbContext>(
        string[] args,
        Action<WebApplicationBuilder> configureServices = null,
        Action<WebApplication> configureApplication = null,
        Func<JsonSerializerOptions> serializerOptions = null,
        bool addTransactionBehavior = true)
    where TDbContext : DbContext =>
        await DomainApplication.InternalRunAsync(args,
            async (wrapper) =>
            {
                wrapper.ConfigureApplicationDefaults(addTransactionBehavior, serializerOptions?.Invoke() ?? JsonHandling.Options)
                    .ConfigureApplicationAuthorizationOptions();

                if (wrapper.AuthOptions.IsAuthServer)
                {
                    wrapper.ConfigureApplicationAuthorizationServer<TDbContext>();
                }
                else
                {
                    wrapper.ConfigureApplicationAuthorizationClient();
                }

                await wrapper
                    .ConfigureServices(configureServices)
                    .ConfigureApplicationDbContext<TDbContext>()
                    .ConfigureSwagger()
                    .BuildDomainApplication()
                    .UseApplicationDefaults()
                    .UseApplicationApiEndpoints()
                    .UseApplicationConfiguration(configureApplication)
                    .UseDatabase<TDbContext>();

                wrapper.Run();
            });


    private static async Task InternalRunAsync(string[] args, Func<DomainApplicationWrapper, Task> run)
    {
        var wrapper = new DomainApplicationWrapper(args);

        try
        {
            await run.Invoke(wrapper);
        }
        catch (Exception ex)
        {
            wrapper.FatalException(ex);

            throw;
        }
        finally
        {
            wrapper.ServiceDown();
        }
    }
}