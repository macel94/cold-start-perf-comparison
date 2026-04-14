namespace Benchmark.ContractTests;

public sealed class ProviderDescriptorContractTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

    [Theory]
    [InlineData("gcp-cloud-run", "europe-west1")]
    [InlineData("aws-lambda", "eu-west-1")]
    [InlineData("azure-container-apps", "westeurope")]
    [InlineData("scaleway-serverless", "fr-par")]
    [InlineData("unikraft-kraftcloud", "fra")]
    public void Provider_descriptor_contains_the_canonical_v1_region_and_runtime(string folder, string region)
    {
        var descriptor = File.ReadAllText(Path.Combine(RepoRoot, "deploy", folder, "descriptor.yaml"));

        Assert.Contains($"region: {region}", descriptor);
        Assert.Contains("runtimeVersion: 10.0.5", descriptor);
        Assert.Contains("startupPath: /api/startup", descriptor);
        Assert.Contains("computePath: /api/compute/matrix", descriptor);
        Assert.Contains("idleWindowMinutes: 15", descriptor);
    }
}
