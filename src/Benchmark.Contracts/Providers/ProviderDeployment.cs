namespace Benchmark.Contracts.Providers;

public sealed record ProviderDeployment
{
    public string ProviderId { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string RuntimeVersion { get; init; } = "8.0.14";
    public string StartupPath { get; init; } = "/api/startup";
    public string ComputePath { get; init; } = "/api/compute/matrix";
    public int IdleWindowMinutes { get; init; } = 15;
    public bool SupportsDeterministicScaleToZeroConfirmation { get; init; }
    public string ScaleToZeroEvidenceStrategy { get; init; } = string.Empty;
    public bool SimulatedScaleToZeroConfirmed { get; init; } = true;
    public string[] ParityNotes { get; init; } = [];
}
