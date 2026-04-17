namespace Benchmark.IntegrationTests;

public sealed class QuickstartSmokeTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

    [Fact]
    public void Repository_docs_describe_a_single_provider_deploy_and_runner_flow()
    {
        var readme = File.ReadAllText(Path.Combine(RepoRoot, "README.md"));
        var quickstart = File.ReadAllText(Path.Combine(RepoRoot, "specs", "001-cross-cloud-benchmark", "quickstart.md"));

        Assert.Contains("dotnet test cold-start-perf-comparison.sln", readme);
        Assert.Contains("terraform -chdir=deploy/gcp-cloud-run init", quickstart);
        Assert.Contains("terraform -chdir=deploy/gcp-cloud-run apply tfplan", quickstart);
        Assert.Contains("terraform -chdir=deploy/gcp-cloud-run output -raw service_url", quickstart);
        Assert.Contains("deploy/unikraft-kraftcloud", quickstart);
        Assert.Contains("benchmark-results", quickstart);
    }
}
