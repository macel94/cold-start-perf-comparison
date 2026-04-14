namespace Benchmark.Contracts.Workloads;

public sealed record WorkloadStep
{
    public string StepId { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public string Intent { get; init; } = string.Empty;
    public string Endpoint { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string? PayloadRef { get; init; }
    public string? Description { get; init; }
    public int ExpectedStatus { get; init; }
}
