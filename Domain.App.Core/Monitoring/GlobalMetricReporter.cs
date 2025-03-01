using Domain.App.Core.Monitoring.Attributes;
using Prometheus;

namespace Domain.App.Core.Monitoring;

[MetricReporter]
public class GlobalMetricReporter
{
    private readonly Gauge _serviceStatusGauge;

    private readonly Gauge _serviceUptimeGauge;

    public GlobalMetricReporter(string environment, string category)
    {
        Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string>
        {
            { nameof(environment), environment },
            { nameof(category), category }
        });

        _serviceStatusGauge = Metrics.CreateGauge(
            "service_status",
            "Service up/down status",
            new GaugeConfiguration { LabelNames = new[] { "service" } });

        _serviceUptimeGauge = Metrics.CreateGauge(
            "service_uptime",
            "Uptime of the service",
            new GaugeConfiguration { LabelNames = new[] { "service" } });

        this.RequestProcessingDuration = Metrics.CreateHistogram(
            "request_processing_duration",
            "Request processing duration",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(1, 2, 16),
                LabelNames = new[] { "request" },
            });

        this.RequestInProcess = Metrics.CreateGauge(
            "request_in_process",
            "Count of currently processing requests",
            new GaugeConfiguration { LabelNames = new[] { "request" } });

        this.Requests = Metrics.CreateCounter(
            "request_count",
            "Count of requests",
            labelNames: new[] { "request" });

        this.Exceptions = Metrics.CreateCounter(
            "exception_count",
            "Number of exceptions",
            labelNames: new[] { "request", "error_code" });
    }

    public Histogram RequestProcessingDuration { get; }

    public Gauge RequestInProcess { get; }

    public Counter Requests { get; }

    public Counter Exceptions { get; }

    public void ServiceUp(string service)
    {
        _serviceStatusGauge.WithLabels(service).Inc();
        _serviceUptimeGauge.WithLabels(service).SetToCurrentTimeUtc();
    }

    public void ServiceDown(string service)
    {
        _serviceStatusGauge.WithLabels(service).Dec();
        _serviceUptimeGauge.WithLabels(service).SetToTimeUtc(DateTimeOffset.UnixEpoch);
    }
}