namespace Benchmark.Contracts.Results;

public sealed record ParityException
{
    public string ParityExceptionId { get; init; } = string.Empty;
    public string ProviderId { get; init; } = string.Empty;
    public string Scope { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Impact { get; init; } = string.Empty;
    public string? RelatedStepId { get; init; }
    public DateTimeOffset RecordedAtUtc { get; init; }
}
