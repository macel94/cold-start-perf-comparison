using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services.ScaleEvidence;

public sealed class AzureContainerAppsScaleEvidenceService : IProviderScaleEvidenceService
{
    public string ProviderId => "azure-container-apps";

    public Task<ScaleEvidenceResult> ConfirmScaleToZeroAsync(ProviderDeployment provider, CancellationToken cancellationToken) =>
        Task.FromResult(provider.SimulatedScaleToZeroConfirmed
            ? new ScaleEvidenceResult(true)
            : new ScaleEvidenceResult(false, "scale-to-zero-timeout", "Azure Container Apps replica evidence was not observed after the fixed idle window.", "Cold-start intent remains annotated but not fully confirmed."));
}
