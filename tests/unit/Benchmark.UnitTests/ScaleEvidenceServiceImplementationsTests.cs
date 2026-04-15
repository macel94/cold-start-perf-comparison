using Benchmark.Contracts.Providers;
using BenchmarkRunner.Services.ScaleEvidence;

namespace Benchmark.UnitTests;

public sealed class ScaleEvidenceServiceImplementationsTests
{
    [Fact]
    public async Task Aws_lambda_reports_a_documented_parity_exception()
    {
        var result = await new AwsLambdaScaleEvidenceService().ConfirmScaleToZeroAsync(CreateProvider("aws-lambda", false), CancellationToken.None);

        Assert.False(result.ScaleToZeroConfirmed);
        Assert.Equal("no-zero-confirmation", result.ExceptionType);
    }

    [Fact]
    public async Task Deterministic_services_can_confirm_or_timeout_without_throwing()
    {
        var cloudRun = await new CloudRunScaleEvidenceService().ConfirmScaleToZeroAsync(CreateProvider("gcp-cloud-run", true), CancellationToken.None);
        var azure = await new AzureContainerAppsScaleEvidenceService().ConfirmScaleToZeroAsync(CreateProvider("azure-container-apps", false), CancellationToken.None);
        var scaleway = await new ScalewayScaleEvidenceService().ConfirmScaleToZeroAsync(CreateProvider("scaleway-serverless-containers", true), CancellationToken.None);
        var unikraft = await new UnikraftKraftCloudScaleEvidenceService().ConfirmScaleToZeroAsync(CreateProvider("unikraft-kraftcloud", true), CancellationToken.None);

        Assert.True(cloudRun.ScaleToZeroConfirmed);
        Assert.False(azure.ScaleToZeroConfirmed);
        Assert.Equal("scale-to-zero-timeout", azure.ExceptionType);
        Assert.True(scaleway.ScaleToZeroConfirmed);
        Assert.True(unikraft.ScaleToZeroConfirmed);
    }

    private static ProviderDeployment CreateProvider(string providerId, bool simulatedScaleConfirmed) =>
        new()
        {
            ProviderId = providerId,
            DisplayName = providerId,
            Region = "test-region",
            BaseUrl = "https://example.test",
            RuntimeVersion = "10.0.5",
            StartupPath = "/api/startup",
            ComputePath = "/api/compute/matrix",
            IdleWindowMinutes = 15,
            SupportsDeterministicScaleToZeroConfirmation = simulatedScaleConfirmed,
            SimulatedScaleToZeroConfirmed = simulatedScaleConfirmed,
            ScaleToZeroEvidenceStrategy = "test"
        };
}
