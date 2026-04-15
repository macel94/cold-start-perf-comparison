using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Results;
using Benchmark.Contracts.Workloads;

namespace BenchmarkRunner.Services;

public sealed class ResultRecordFactory
{
    public ResultRecord CreateStartupRecord(
        string runId,
        ProviderDeployment provider,
        WorkloadStep step,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        double latencyMs,
        int httpStatus,
        bool scaleToZeroConfirmed,
        List<string>? annotationRefs = null,
        string? errorType = null,
        string? errorDetail = null) =>
        new()
        {
            RunId = runId,
            ProviderId = provider.ProviderId,
            Region = provider.Region,
            StepId = step.StepId,
            Sequence = step.Sequence,
            Intent = step.Intent,
            Endpoint = step.Endpoint,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            LatencyMs = latencyMs,
            HttpStatus = httpStatus,
            Correctness = "not-applicable",
            ScaleToZeroConfirmed = scaleToZeroConfirmed,
            AnnotationRefs = annotationRefs,
            ErrorType = errorType,
            ErrorDetail = errorDetail
        };

    public ResultRecord CreateComputeRecord(
        string runId,
        ProviderDeployment provider,
        WorkloadStep step,
        DateTimeOffset startedAtUtc,
        DateTimeOffset completedAtUtc,
        double latencyMs,
        int httpStatus,
        string correctness,
        object? responseBody,
        string? errorType = null,
        string? errorDetail = null) =>
        new()
        {
            RunId = runId,
            ProviderId = provider.ProviderId,
            Region = provider.Region,
            StepId = step.StepId,
            Sequence = step.Sequence,
            Intent = step.Intent,
            Endpoint = step.Endpoint,
            StartedAtUtc = startedAtUtc,
            CompletedAtUtc = completedAtUtc,
            LatencyMs = latencyMs,
            HttpStatus = httpStatus,
            Correctness = correctness,
            ResponseBody = responseBody,
            ErrorType = errorType,
            ErrorDetail = errorDetail
        };
}
