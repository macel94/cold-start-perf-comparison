using Benchmark.Contracts.Payloads;
using Benchmark.Contracts.Results;
using Benchmark.Contracts.Validation;
using Benchmark.Contracts.Workloads;
using BenchmarkApp.Models;

namespace Benchmark.ContractTests;

public sealed class BenchmarkSchemasContractTests
{
    private static readonly string RepoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", ".."));

    [Fact]
    public void Workload_artifact_is_schema_valid_and_contains_cold_and_warm_steps()
    {
        var workloadJson = File.ReadAllText(Path.Combine(RepoRoot, "workloads", "v1", "cross-cloud-sequential.json"));
        var workload = System.Text.Json.JsonSerializer.Deserialize<WorkloadDefinition>(workloadJson, new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web))!;

        var errors = new WorkloadSchemaValidator().Validate(workload);

        Assert.Empty(errors);
        Assert.Contains(workload.Steps, step => step.Intent == "cold");
        Assert.Contains(workload.Steps, step => step.Intent == "warm");
    }

    [Fact]
    public void Sample_result_envelope_matches_the_normalized_contract_rules()
    {
        var payload = MatrixPayloadFactory.CreatePayload("matrix-100x100", 100);
        var run = new BenchmarkRun
        {
            RunId = "run-001",
            StartedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            WorkloadVersion = "v1.0.0",
            WorkloadFileHash = "abc",
            ApiContractVersion = "1.0.0",
            ResultSchemaVersion = "1.0.0",
            Providers = ["gcp-cloud-run"],
            RunnerVersion = "1.0.0",
            Status = "completed",
            Records =
            [
                new ResultRecord
                {
                    RunId = "run-001",
                    ProviderId = "gcp-cloud-run",
                    Region = "europe-west1",
                    StepId = "cold-start-001",
                    Sequence = 1,
                    Intent = "cold",
                    Endpoint = "startup",
                    StartedAtUtc = DateTimeOffset.UtcNow,
                    CompletedAtUtc = DateTimeOffset.UtcNow,
                    LatencyMs = 12,
                    HttpStatus = 200,
                    Correctness = "not-applicable",
                    ScaleToZeroConfirmed = true
                },
                new ResultRecord
                {
                    RunId = "run-001",
                    ProviderId = "gcp-cloud-run",
                    Region = "europe-west1",
                    StepId = "warm-compute-100-001",
                    Sequence = 2,
                    Intent = "warm",
                    Endpoint = "compute",
                    StartedAtUtc = DateTimeOffset.UtcNow,
                    CompletedAtUtc = DateTimeOffset.UtcNow,
                    LatencyMs = 30,
                    HttpStatus = 200,
                    Correctness = "passed",
                    ResponseBody = new MatrixComputeResponse(payload.PayloadId, payload.Dimension, MatrixPayloadFactory.Multiply(payload))
                }
            ],
            SummaryMetrics =
            [
                new SummaryMetric
                {
                    ProviderId = "gcp-cloud-run",
                    Intent = "cold",
                    SampleCount = 1,
                    MinLatencyMs = 12,
                    P50LatencyMs = 12,
                    P95LatencyMs = 12,
                    P99LatencyMs = 12,
                    MaxLatencyMs = 12,
                    ErrorCount = 0,
                    ParityExceptionCount = 0
                },
                new SummaryMetric
                {
                    ProviderId = "gcp-cloud-run",
                    Intent = "warm",
                    SampleCount = 1,
                    MinLatencyMs = 30,
                    P50LatencyMs = 30,
                    P95LatencyMs = 30,
                    P99LatencyMs = 30,
                    MaxLatencyMs = 30,
                    ErrorCount = 0,
                    ParityExceptionCount = 0
                }
            ]
        };

        var errors = new ResultsSchemaValidator().Validate(run);
        Assert.Empty(errors);
    }
}
