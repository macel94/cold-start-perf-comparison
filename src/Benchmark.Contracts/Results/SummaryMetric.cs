namespace Benchmark.Contracts.Results;

public sealed record SummaryMetric
{
    public string ProviderId { get; init; } = string.Empty;
    public string Intent { get; init; } = string.Empty;
    public int SampleCount { get; init; }
    public double MinLatencyMs { get; init; }
    public double P50LatencyMs { get; init; }
    public double P95LatencyMs { get; init; }
    public double P99LatencyMs { get; init; }
    public double MaxLatencyMs { get; init; }
    public int ErrorCount { get; init; }
    public int ParityExceptionCount { get; init; }
}
