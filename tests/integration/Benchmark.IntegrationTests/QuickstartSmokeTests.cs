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
        Assert.Contains("dotnet run --project src/BenchmarkRunner", readme);
        Assert.Contains("deploy/gcp-cloud-run", quickstart);
        Assert.Contains("benchmark-results", quickstart);
    }
}
