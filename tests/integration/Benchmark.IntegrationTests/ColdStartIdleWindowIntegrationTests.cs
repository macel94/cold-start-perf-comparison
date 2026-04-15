using System.Net;
using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Workloads;
using BenchmarkRunner.Services;
using BenchmarkRunner.Services.ScaleEvidence;

namespace Benchmark.IntegrationTests;

public sealed class ColdStartIdleWindowIntegrationTests
{
    [Fact]
    public async Task Executor_enforces_the_fixed_idle_window_and_records_a_parity_exception_when_confirmation_is_missing()
    {
        var observedDelays = new List<TimeSpan>();
        using var httpClient = new HttpClient(new RecordingHandler([new HttpResponseMessage(HttpStatusCode.OK)]));
        var parityRecorder = new ParityExceptionRecorder();
        var executor = new SequentialWorkloadExecutor(
            httpClient,
            new ColdStartIdleWindowCoordinator((delay, _) =>
            {
                observedDelays.Add(delay);
                return Task.CompletedTask;
            }),
            [new AwsLambdaScaleEvidenceService()],
            parityRecorder,
            new ComputeStepExecutor(httpClient, new MatrixResultVerifier(), new ResultRecordFactory()),
            new ResultRecordFactory(),
            new RunFailurePolicy());

        var workload = new WorkloadDefinition
        {
            WorkloadVersion = "v1",
            Description = "cold only",
            Steps = [new WorkloadStep { StepId = "cold", Sequence = 1, Intent = "cold", Endpoint = "startup", Method = "GET", ExpectedStatus = 200 }]
        };

        var provider = new ProviderDeployment
        {
            ProviderId = "aws-lambda",
            DisplayName = "AWS Lambda",
            Region = "eu-west-1",
            BaseUrl = "https://example.test",
            RuntimeVersion = "10.0.5",
            StartupPath = "/api/startup",
            ComputePath = "/api/compute/matrix",
            IdleWindowMinutes = 15,
            SupportsDeterministicScaleToZeroConfirmation = false,
            SimulatedScaleToZeroConfirmed = false,
            ScaleToZeroEvidenceStrategy = "inference"
        };

        var records = await executor.ExecuteAsync("run-001", [provider], workload, CancellationToken.None);

        Assert.Single(observedDelays);
        Assert.Equal(TimeSpan.FromMinutes(15), observedDelays[0]);
        Assert.False(records[0].ScaleToZeroConfirmed);
        Assert.NotNull(records[0].AnnotationRefs);
        Assert.Single(parityRecorder.Exceptions);
    }
}
