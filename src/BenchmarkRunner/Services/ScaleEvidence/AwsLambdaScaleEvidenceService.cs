using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services.ScaleEvidence;

public sealed class AwsLambdaScaleEvidenceService : IProviderScaleEvidenceService
{
    public string ProviderId => "aws-lambda";

    public Task<ScaleEvidenceResult> ConfirmScaleToZeroAsync(ProviderDeployment provider, CancellationToken cancellationToken) =>
        Task.FromResult(new ScaleEvidenceResult(
            false,
            "no-zero-confirmation",
            "AWS Lambda does not expose deterministic public scale-to-zero confirmation in v1.",
            "Cold-start intent is inferred after the uniform idle window and should be interpreted with a parity caveat."));
}
