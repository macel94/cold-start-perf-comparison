namespace Benchmark.Contracts.Results;

public sealed record ResultRecord
{
    public string RunId { get; init; } = string.Empty;
    public string ProviderId { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string StepId { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public string Intent { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public DateTimeOffset StartedAtUtc { get; init; }
    public DateTimeOffset CompletedAtUtc { get; init; }
    public double LatencyMs { get; init; }
    public int HttpStatus { get; init; }
    public string Correctness { get; init; } = string.Empty;
    public object? ResponseBody { get; init; }
    public string? ErrorType { get; init; }
    public string? ErrorDetail { get; init; }
    public bool? ScaleToZeroConfirmed { get; init; }
    public List<string>? AnnotationRefs { get; init; }
}
