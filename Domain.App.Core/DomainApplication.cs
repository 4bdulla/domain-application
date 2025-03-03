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
        await RunApplication(args,
            async wrapper => await wrapper
                .ConfigureApplicationDefaults(false, serializerOptions?.Invoke() ?? JsonHandling.Options)
                .ConfigureApplicationAuthorizationOptions()
                .ConfigureApplicationAuthorizationClient()
                .ConfigureCustomServices(configureServices)
                .ConfigureSwagger()
                .BuildDomainApplication()
                .UseApplicationDefaults()
                .UseApplicationApiEndpoints()
                .UseCustomConfigureApplication(configureApplication)
                .Run());

    public static async Task RunWithDbAsync<TDbContext>(
        string[] args,
        Action<WebApplicationBuilder> configureServices = null,
        Action<WebApplication> configureApplication = null,
        Func<JsonSerializerOptions> serializerOptions = null,
        bool addTransactionBehavior = true)
    where TDbContext : DbContext =>
        await RunApplication(args,
            async wrapper =>
            {
                await wrapper.ConfigureApplicationDefaults(addTransactionBehavior, serializerOptions?.Invoke() ?? JsonHandling.Options)
                    .ConfigureApplicationAuthorizationOptions()
                    .ConfigureApplicationAuthorization<TDbContext>()
                    .ConfigureCustomServices(configureServices)
                    .ConfigureApplicationDbContext<TDbContext>()
                    .ConfigureSwagger()
                    .BuildDomainApplication()
                    .UseApplicationDefaults()
                    .UseApplicationApiEndpoints()
                    .UseCustomConfigureApplication(configureApplication)
                    .UseDatabase<TDbContext>();

                await wrapper.Run();
            });


    private static async Task RunApplication(string[] args, Func<DomainApplicationWrapper, Task> run)
    {
        DomainApplicationWrapper wrapper = new(args);

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