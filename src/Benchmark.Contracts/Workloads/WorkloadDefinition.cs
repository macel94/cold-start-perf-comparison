namespace Benchmark.Contracts.Workloads;

public sealed record WorkloadDefinition
{
    public string WorkloadVersion { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<PayloadDefinition> PayloadCatalog { get; init; } = [];
    public List<WorkloadStep> Steps { get; init; } = [];
}
