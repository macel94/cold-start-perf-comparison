namespace Benchmark.Contracts.Results;

public sealed record BenchmarkRun
{
    public string RunId { get; init; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset? CompletedAtUtc { get; init; }
    public string WorkloadVersion { get; init; } = string.Empty;
    public string WorkloadFileHash { get; init; } = string.Empty;
    public string ApiContractVersion { get; init; } = string.Empty;
    public string ResultSchemaVersion { get; init; } = string.Empty;
    public List<string> Providers { get; init; } = [];
    public string RunnerVersion { get; init; } = string.Empty;
    public string? NetworkLocationLabel { get; init; }
    public List<ResultRecord> Records { get; init; } = [];
    public List<ParityException> ParityExceptions { get; init; } = [];
    public List<SummaryMetric> SummaryMetrics { get; init; } = [];
    public string Status { get; init; } = string.Empty;
}
