using System.Net;
using System.Text;
using System.Text.Json;
using Benchmark.Contracts.Payloads;
using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Workloads;
using BenchmarkRunner.Services;
using BenchmarkRunner.Services.ScaleEvidence;
using BenchmarkApp.Models;

namespace Benchmark.IntegrationTests;

public sealed class SequentialWorkloadExecutorIntegrationTests
{
    [Fact]
    public async Task Executor_runs_steps_in_order_and_emits_records()
    {
        var payload = MatrixPayloadFactory.CreatePayload("matrix-100x100", 100);
        var workload = new WorkloadDefinition
        {
            WorkloadVersion = "v1",
            Description = "test",
            PayloadCatalog = [payload],
            Steps =
            [
                new WorkloadStep { StepId = "cold", Sequence = 1, Intent = "cold", Endpoint = "startup", Method = "GET", ExpectedStatus = 200 },
                new WorkloadStep { StepId = "warm", Sequence = 2, Intent = "warm", Endpoint = "compute", Method = "POST", PayloadRef = payload.PayloadId, ExpectedStatus = 200 }
            ]
        };

        var handler = new RecordingHandler(
        [
            new HttpResponseMessage(HttpStatusCode.OK),
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new MatrixComputeResponse(payload.PayloadId, payload.Dimension, MatrixPayloadFactory.Multiply(payload))),
                    Encoding.UTF8,
                    "application/json")
            }
        ]);

        using var httpClient = new HttpClient(handler);
        var executor = new SequentialWorkloadExecutor(
            httpClient,
            new ColdStartIdleWindowCoordinator((_, _) => Task.CompletedTask),
            [new CloudRunScaleEvidenceService()],
            new ParityExceptionRecorder(),
            new ComputeStepExecutor(httpClient, new MatrixResultVerifier(), new ResultRecordFactory()),
            new ResultRecordFactory(),
            new RunFailurePolicy());

        var provider = CreateProvider("gcp-cloud-run", "europe-west1", true);
        var records = await executor.ExecuteAsync("run-001", [provider], workload, CancellationToken.None);

        Assert.Equal(["/api/startup", "/api/compute/matrix"], handler.RequestUris.Select(uri => uri.AbsolutePath));
        Assert.Equal(2, records.Count);
        Assert.Equal("cold", records[0].Intent);
        Assert.Equal("warm", records[1].Intent);
    }

    private static ProviderDeployment CreateProvider(string providerId, string region, bool simulatedScale) =>
        new()
        {
            ProviderId = providerId,
            DisplayName = providerId,
            Region = region,
            BaseUrl = "https://example.test",
            RuntimeVersion = "10.0.5",
            StartupPath = "/api/startup",
            ComputePath = "/api/compute/matrix",
            IdleWindowMinutes = 15,
            SupportsDeterministicScaleToZeroConfirmation = true,
            SimulatedScaleToZeroConfirmed = simulatedScale,
            ScaleToZeroEvidenceStrategy = "test"
        };
}

internal sealed class RecordingHandler : HttpMessageHandler
{
    private readonly Queue<HttpResponseMessage> _responses;

    public RecordingHandler(IEnumerable<HttpResponseMessage> responses)
    {
        _responses = new Queue<HttpResponseMessage>(responses);
    }

    public List<Uri> RequestUris { get; } = [];

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUris.Add(request.RequestUri!);
        return Task.FromResult(_responses.Dequeue());
    }
}
