using System.Net;
using System.Net.Http.Json;
using Benchmark.Contracts.Payloads;
using BenchmarkApp.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Benchmark.ContractTests;

public sealed class BenchmarkAppOpenApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));
    private readonly WebApplicationFactory<Program> _factory;

    public BenchmarkAppOpenApiContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Startup_endpoint_matches_the_documented_contract()
    {
        using var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/startup");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.Equal("ready", payload?["status"]);
    }

    [Fact]
    public async Task Compute_endpoint_matches_the_documented_contract()
    {
        using var client = _factory.CreateClient();
        var payload = MatrixPayloadFactory.CreatePayload("matrix-100x100", 100);

        var response = await client.PostAsJsonAsync(
            "/api/compute/matrix",
            new MatrixComputeRequest(payload.PayloadId, payload.Dimension, payload.LeftMatrix, payload.RightMatrix));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<MatrixComputeResponse>();
        Assert.NotNull(body);
        Assert.Equal("matrix-100x100", body.PayloadId);
        Assert.Equal(100, body.Dimension);
        Assert.Equal(100, body.ResultMatrix.Length);
    }

    [Fact]
    public void OpenApi_document_contains_the_fixed_v1_paths_and_versions()
    {
        var contract = File.ReadAllText(Path.Combine(RepoRoot, "specs", "001-cross-cloud-benchmark", "contracts", "benchmark-app.openapi.yaml"));

        Assert.Contains("version: 1.0.0", contract);
        Assert.Contains("/api/startup:", contract);
        Assert.Contains("/api/compute/matrix:", contract);
        Assert.Contains("const: matrix-100x100", contract);
        Assert.Contains("const: matrix-200x200", contract);
    }
}
