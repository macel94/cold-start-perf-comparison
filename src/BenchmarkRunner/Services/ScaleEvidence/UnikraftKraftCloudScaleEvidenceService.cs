using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services.ScaleEvidence;

public sealed class UnikraftKraftCloudScaleEvidenceService : IProviderScaleEvidenceService
{
    public string ProviderId => "unikraft-kraftcloud";

    public Task<ScaleEvidenceResult> ConfirmScaleToZeroAsync(ProviderDeployment provider, CancellationToken cancellationToken) =>
        Task.FromResult(provider.SimulatedScaleToZeroConfirmed
            ? new ScaleEvidenceResult(true)
            : new ScaleEvidenceResult(false, "scale-to-zero-timeout", "KraftCloud standby or instance-state evidence was not observed after the fixed idle window.", "Cold-start intent remains annotated but not fully confirmed."));
}
