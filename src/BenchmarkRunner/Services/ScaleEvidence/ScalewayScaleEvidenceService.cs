using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services.ScaleEvidence;

public sealed class ScalewayScaleEvidenceService : IProviderScaleEvidenceService
{
    public string ProviderId => "scaleway-serverless-containers";

    public Task<ScaleEvidenceResult> ConfirmScaleToZeroAsync(ProviderDeployment provider, CancellationToken cancellationToken) =>
        Task.FromResult(provider.SimulatedScaleToZeroConfirmed
            ? new ScaleEvidenceResult(true)
            : new ScaleEvidenceResult(false, "scale-to-zero-timeout", "Scaleway instance evidence was not observed after the fixed idle window.", "Cold-start intent remains annotated but not fully confirmed."));
}
