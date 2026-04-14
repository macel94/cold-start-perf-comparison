using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services.ScaleEvidence;

public interface IProviderScaleEvidenceService
{
    string ProviderId { get; }

    Task<ScaleEvidenceResult> ConfirmScaleToZeroAsync(ProviderDeployment provider, CancellationToken cancellationToken);
}
