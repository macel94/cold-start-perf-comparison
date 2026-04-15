namespace Benchmark.Contracts.Workloads;

public sealed record PayloadDefinition
{
    public string PayloadId { get; init; } = string.Empty;
    public int Dimension { get; init; }
    public double[][] LeftMatrix { get; init; } = [];
    public double[][] RightMatrix { get; init; } = [];
    public string ContentHash { get; init; } = string.Empty;
}
