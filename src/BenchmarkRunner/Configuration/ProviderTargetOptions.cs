using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Configuration;

public sealed class ProviderTargetOptions
{
    public const string SectionName = "ProviderTargets";

    public List<ProviderDeployment> Providers { get; init; } = [];
}
