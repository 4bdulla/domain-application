using System.Text.Json;

using CorrelationId;

using Domain.App.Core.Auth;
using Domain.App.Core.Configuration;
using Domain.App.Core.Database;
using Domain.App.Core.Integration;
using Domain.App.Core.Logging;
using Domain.App.Core.Mediator;
using Domain.App.Core.Monitoring;
using Domain.App.Core.Swagger;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;


namespace Domain.App.Core.Utility;

public static class DomainApplicationBuilderExtensions
{
    public static DomainApplicationWrapper ConfigureApplicationDefaults(
        this DomainApplicationWrapper wrapper,
        bool addTransactionBehavior,
        JsonSerializerOptions settings)
    {
        wrapper.Builder.LoadConfiguration();
        wrapper.Builder.Services.AddMemoryCache();
        wrapper.Builder.Services.AddControllers();
        wrapper.Builder.Services.AddEndpointsApiExplorer();
        wrapper.Builder.Services.AddHttpContextAccessor();

        // todo ensure same tracing across different services
        wrapper.Builder.Services.AddOpenTelemetry()
            .ConfigureResource(b => b.AddService(wrapper.Builder.Environment.ApplicationName))
            .WithTracing(b => b.AddAspNetCoreInstrumentation());

        wrapper.Builder.AddHealthChecks();
        wrapper.Builder.AddMetricReporters();
        wrapper.Builder.ConfigureSerilog();
        wrapper.Builder.ConfigureCorrelationId();
        wrapper.Builder.AddJwtGenerator();
        wrapper.Builder.AddFluentValidation();
        wrapper.Builder.AddMediator(addTransactionBehavior);
        wrapper.Builder.ConfigureRefitClients(settings);

        return wrapper;
    }


    public static DomainApplicationWrapper ConfigureApplicationDbContext<TDbContext>(this DomainApplicationWrapper wrapper)
    where TDbContext : DbContext
    {
        wrapper.DatabaseOptions = wrapper.Builder.ConfigureSqlDbOptions();

        // todo add switch and options for different db setups
        wrapper.Builder.AddSqlDb<TDbContext>(wrapper.DatabaseOptions);
        wrapper.Builder.AddAuditor();

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureApplicationAuthorizationOptions(this DomainApplicationWrapper wrapper)
    {
        wrapper.AuthOptions = wrapper.Builder.ConfigureAuthOptions();

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureApplicationAuthorization<TDbContext>(this DomainApplicationWrapper wrapper)
    where TDbContext : DbContext
    {
        if (wrapper.AuthOptions.IsAuthServer)
        {
            wrapper.ConfigureApplicationAuthorizationServer<TDbContext>();
        }
        else
        {
            wrapper.ConfigureApplicationAuthorizationClient();
        }

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureApplicationAuthorizationServer<TDbContext>(this DomainApplicationWrapper wrapper)
    where TDbContext : DbContext
    {
        wrapper.Builder.ConfigureAuthServer<TDbContext>(wrapper.AuthOptions);

        Log.Debug("auth server configured with {@JwtOptions}", wrapper.AuthOptions.Jwt);

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureApplicationAuthorizationClient(this DomainApplicationWrapper wrapper)
    {
        wrapper.Builder.ConfigureAuthClient(wrapper.AuthOptions);

        Log.Debug("auth client configured: {ServerUrl}", wrapper.AuthOptions.Server);

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureSwagger(this DomainApplicationWrapper wrapper)
    {
        wrapper.RoutingOptions = wrapper.Builder.ConfigureRoutingOptions();

        wrapper.Builder.AddSwagger(wrapper.AuthOptions.UseAuthInDevelopmentEnvironment);

        return wrapper;
    }

    public static DomainApplicationWrapper ConfigureCustomServices(
        this DomainApplicationWrapper wrapper,
        Action<WebApplicationBuilder> configureServices)
    {
        configureServices?.Invoke(wrapper.Builder);

        return wrapper;
    }


    public static async Task<DomainApplicationWrapper> UseDatabase<TDbContext>(this DomainApplicationWrapper wrapper)
    where TDbContext : DbContext
    {
        bool dbCreated = await wrapper.App.EnsureApplicationDbCreatedAsync<TDbContext>(wrapper.DatabaseOptions);

        Log.Debug("database created: {DbCreated}", dbCreated);

        await wrapper.App.UseApplicationRoles(wrapper.AuthOptions);

        return wrapper;
    }

    public static DomainApplicationWrapper UseApplicationDefaults(this DomainApplicationWrapper wrapper)
    {
        wrapper.App.UseCorrelationId();
        wrapper.App.UseRequestLogging();
        wrapper.App.UseSwagger();
        wrapper.App.UseSwaggerUI();
        wrapper.App.UseAuthDiscovery();
        wrapper.App.UseApplicationExceptionHandler();

        return wrapper;
    }

    public static DomainApplicationWrapper UseApplicationApiEndpoints(this DomainApplicationWrapper wrapper)
    {
        bool useApiValidation = wrapper.App.Configuration.GetValue("UseApiValidation", true);

        wrapper.App.MapApiRoutes(useApiValidation, wrapper.AuthOptions.UseAuthInDevelopmentEnvironment);

        wrapper.App.UseMonitoring();

        return wrapper;
    }

    public static DomainApplicationWrapper UseCustomConfigureApplication(
        this DomainApplicationWrapper wrapper,
        Action<WebApplication> configureApplication)
    {
        configureApplication?.Invoke(wrapper.App);

        return wrapper;
    }
}