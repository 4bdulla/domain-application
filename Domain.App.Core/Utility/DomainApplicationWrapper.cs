using Domain.App.Core.FastFailing;
using Domain.App.Core.Monitoring;
using Domain.App.Core.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        string category = this.Builder.Configuration.GetValue<string>("Category");

        if (string.IsNullOrWhiteSpace(category))
            throw new InvalidOperationException($"{nameof(category)} setting is not configured!");

        this.Reporter = new GlobalMetricReporter(this.Builder.Environment.EnvironmentName, category);
    }


    public string[] Args { get; set; }
    public GlobalMetricReporter Reporter { get; set; }
    public WebApplicationBuilder Builder { get; set; }
    public WebApplication App { get; set; }
    public AuthOptions AuthOptions { get; set; }
    public SqlDbOptions SqlDbOptions { get; set; }
    public RoutingOptions RoutingOptions { get; set; }

    public DomainApplicationWrapper BuildDomainApplication()
    {
        this.Builder.Services.AddSingleton(this.Reporter);

        this.Builder.AddFastFailing();

        this.App = this.Builder.Build();

        return this;
    }

    public void Run()
    {
        this.LogApplicationConfiguration();

        this.Reporter.ServiceUp(this.Builder.Environment.ApplicationName);

        this.App.RunWithFastFailing();
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
        if (this.SqlDbOptions is not null)
        {
            Log.Debug("database options: {@DbOptions}", this.SqlDbOptions);
        }

        if (this.AuthOptions is not null)
        {
            Log.Debug("auth options: {@AuthOptions}", this.AuthOptions);
        }

        if (this.RoutingOptions is not null)
        {
            Log.Debug("routing options: {@RoutingOptions}", this.RoutingOptions);
        }
    }
}