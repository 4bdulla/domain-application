using Domain.App.Core.Monitoring.Attributes;

using Prometheus;


namespace Domain.App.Core.Monitoring;

[MetricReporter]
public class GlobalMetricReporter
{
    private readonly Gauge _serviceStatusGauge;

    private readonly Gauge _serviceUptimeGauge;

    public GlobalMetricReporter(string environment)
    {
        Metrics.DefaultRegistry.SetStaticLabels(new Dictionary<string, string> { { nameof(environment), environment }, });

        _serviceStatusGauge = Metrics.CreateGauge(
            "service_status",
            "Service up/down status",
            new GaugeConfiguration { LabelNames = ["service"] });

        _serviceUptimeGauge = Metrics.CreateGauge(
            "service_uptime",
            "Uptime of the service",
            new GaugeConfiguration { LabelNames = ["service"] });

        this.RequestProcessingDuration = Metrics.CreateHistogram(
            "mediator_request_processing_duration",
            "Mediator request processing duration",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(1, 2, 16),
                LabelNames = ["request"]
            });

        this.RequestInProcess = Metrics.CreateGauge(
            "mediator_request_in_process",
            "Count of currently processing Mediator requests",
            new GaugeConfiguration { LabelNames = ["request"] });

        this.Requests = Metrics.CreateCounter(
            "mediator_request_count",
            "Count of Mediator requests",
            labelNames: ["request"]);

        this.Exceptions = Metrics.CreateCounter(
            "exception_count",
            "Number of exceptions",
            labelNames: ["request", "error_code"]);
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