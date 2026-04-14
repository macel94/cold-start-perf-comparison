using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Results;
using Benchmark.Contracts.Workloads;

namespace BenchmarkRunner.Services;

public sealed class RunEnvelopeBuilder
{
    public BenchmarkRun CreateStartedRun(
        WorkloadDefinition workload,
        IReadOnlyList<ProviderDeployment> providers,
        string workloadHash,
        string apiContractVersion,
        string resultSchemaVersion,
        string runnerVersion,
        string? networkLocationLabel)
    {
        return new BenchmarkRun
        {
            RunId = Guid.NewGuid().ToString("N"),
            StartedAtUtc = DateTimeOffset.UtcNow,
            WorkloadVersion = workload.WorkloadVersion,
            WorkloadFileHash = workloadHash,
            ApiContractVersion = apiContractVersion,
            ResultSchemaVersion = resultSchemaVersion,
            Providers = providers.Select(provider => provider.ProviderId).ToList(),
            RunnerVersion = runnerVersion,
            NetworkLocationLabel = networkLocationLabel,
            Status = "running"
        };
    }

    public BenchmarkRun Complete(
        BenchmarkRun run,
        IReadOnlyList<ResultRecord> records,
        IReadOnlyList<ParityException> parityExceptions,
        IReadOnlyList<SummaryMetric> summaryMetrics,
        string status)
    {
        return run with
        {
            CompletedAtUtc = DateTimeOffset.UtcNow,
            Records = records.ToList(),
            ParityExceptions = parityExceptions.ToList(),
            SummaryMetrics = summaryMetrics.ToList(),
            Status = status
        };
    }
}
