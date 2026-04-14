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

public sealed class ComputeProbeIntegrationTests
{
    [Fact]
    public async Task Warm_compute_failures_are_recorded_without_stopping_following_steps()
    {
        var payload = MatrixPayloadFactory.CreatePayload("matrix-100x100", 100);
        var workload = new WorkloadDefinition
        {
            WorkloadVersion = "v1",
            Description = "compute continuation",
            PayloadCatalog = [payload],
            Steps =
            [
                new WorkloadStep { StepId = "warm-1", Sequence = 1, Intent = "warm", Endpoint = "compute", Method = "POST", PayloadRef = payload.PayloadId, ExpectedStatus = 200 },
                new WorkloadStep { StepId = "warm-2", Sequence = 2, Intent = "warm", Endpoint = "compute", Method = "POST", PayloadRef = payload.PayloadId, ExpectedStatus = 200 }
            ]
        };

        using var httpClient = new HttpClient(new RecordingHandler(
        [
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("{\"error\":\"invalid-payload\"}", Encoding.UTF8, "application/json")
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(new MatrixComputeResponse(payload.PayloadId, payload.Dimension, MatrixPayloadFactory.Multiply(payload))),
                    Encoding.UTF8,
                    "application/json")
            }
        ]));

        var executor = new SequentialWorkloadExecutor(
            httpClient,
            new ColdStartIdleWindowCoordinator((_, _) => Task.CompletedTask),
            [new CloudRunScaleEvidenceService()],
            new ParityExceptionRecorder(),
            new ComputeStepExecutor(httpClient, new MatrixResultVerifier(), new ResultRecordFactory()),
            new ResultRecordFactory(),
            new RunFailurePolicy());

        var provider = new ProviderDeployment
        {
            ProviderId = "gcp-cloud-run",
            DisplayName = "GCP Cloud Run",
            Region = "europe-west1",
            BaseUrl = "https://example.test",
            RuntimeVersion = "8.0.14",
            StartupPath = "/api/startup",
            ComputePath = "/api/compute/matrix",
            IdleWindowMinutes = 15,
            SupportsDeterministicScaleToZeroConfirmation = true,
            SimulatedScaleToZeroConfirmed = true,
            ScaleToZeroEvidenceStrategy = "metrics"
        };

        var records = await executor.ExecuteAsync("run-001", [provider], workload, CancellationToken.None);

        Assert.Equal(2, records.Count);
        Assert.True(records[0].HttpStatus >= 400);
        Assert.Equal("failed", records[0].Correctness);
        Assert.Equal("passed", records[1].Correctness);
    }
}
