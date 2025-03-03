using Domain.App.Core.Monitoring;
using Domain.App.Core.Options;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;


namespace Domain.App.Core.Utility;

public class DomainApplicationWrapper
{
    public DomainApplicationWrapper(string[] args)
    {
        Log.Debug("application starting...");

        this.Args = args;

        this.Builder = WebApplication.CreateBuilder(args);

        Log.Debug("hosting environment: {Environment}", this.Builder.Environment.EnvironmentName);

        this.Reporter = new DefaultMetricReporter(this.Builder.Environment.EnvironmentName);

        this.Builder.Services.AddSingleton(this.Reporter);
    }


    public string[] Args { get; set; }
    public DefaultMetricReporter Reporter { get; set; }
    public WebApplicationBuilder Builder { get; set; }
    public WebApplication App { get; set; }
    public AuthOptions AuthOptions { get; set; }
    public DatabaseOptions DatabaseOptions { get; set; }
    public RoutingOptions RoutingOptions { get; set; }

    public DomainApplicationWrapper BuildDomainApplication()
    {
        this.Builder.Host.UseDefaultServiceProvider(options => options.ValidateOnBuild = true);
        this.App = this.Builder.Build();

        return this;
    }

    public async Task Run()
    {
        this.LogApplicationConfiguration();

        this.Reporter.ServiceUp(this.Builder.Environment.ApplicationName);

        await this.App.RunAsync();
    }

    public void FatalException(Exception ex)
    {
        Log.Fatal(ex, "application failed to start: {Message}", ex.Message);

        this.Reporter.Exceptions.WithLabels("__STARTUP", "other").Inc();
    }

    public void ServiceDown()
    {
        this.Reporter.ServiceDown(this.Builder.Environment.ApplicationName);

        Log.Debug("application stopping...");
    }

    private void LogApplicationConfiguration()
    {
        if (this.DatabaseOptions is not null)
        {
            Log.Debug("database options: {@Options}", this.DatabaseOptions);
        }

        if (this.AuthOptions is not null)
        {
            Log.Debug("auth options: {@Options}", this.AuthOptions);
        }

        if (this.RoutingOptions is not null)
        {
            Log.Debug("routing options: {@Options}", this.RoutingOptions);
        }
    }
}