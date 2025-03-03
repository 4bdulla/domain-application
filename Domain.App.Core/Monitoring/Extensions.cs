using System.Net;

using Domain.App.Core.Monitoring.Attributes;
using Domain.App.Core.Utility;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using Prometheus;


namespace Domain.App.Core.Monitoring;

public static class Extensions
{
    public static void AddMetricReporters(this WebApplicationBuilder builder)
    {
        Type[] types = AppDomain.CurrentDomain.GetAssemblies().GetTypesByAttribute<MetricReporterAttribute>().ToArray();

        foreach (Type type in types)
        {
            builder.Services.AddSingleton(type);
        }
    }

    public static void AddHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks().ForwardToPrometheus(new PrometheusHealthCheckPublisherOptions());
    }

    public static void UseMonitoring(this WebApplication app)
    {
        app.UseHealthChecks("/healthcheck");

        app.UseHttpMetrics(o =>
        {
            o.ReduceStatusCodeCardinality();
            o.ConfigureMeasurements(c => c.ExemplarPredicate = context => context.Response.StatusCode != (int)HttpStatusCode.OK);
        });

        app.MapMetrics();
    }
}