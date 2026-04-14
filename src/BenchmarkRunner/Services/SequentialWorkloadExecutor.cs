using System.Diagnostics;
using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Results;
using Benchmark.Contracts.Workloads;
using BenchmarkRunner.Services.ScaleEvidence;

namespace BenchmarkRunner.Services;

public sealed class SequentialWorkloadExecutor
{
    private readonly HttpClient _httpClient;
    private readonly ColdStartIdleWindowCoordinator _idleWindowCoordinator;
    private readonly IReadOnlyDictionary<string, IProviderScaleEvidenceService> _scaleEvidenceServices;
    private readonly ParityExceptionRecorder _parityExceptionRecorder;
    private readonly ComputeStepExecutor _computeStepExecutor;
    private readonly ResultRecordFactory _resultRecordFactory;
    private readonly RunFailurePolicy _runFailurePolicy;

    public SequentialWorkloadExecutor(
        HttpClient httpClient,
        ColdStartIdleWindowCoordinator idleWindowCoordinator,
        IEnumerable<IProviderScaleEvidenceService> scaleEvidenceServices,
        ParityExceptionRecorder parityExceptionRecorder,
        ComputeStepExecutor computeStepExecutor,
        ResultRecordFactory resultRecordFactory,
        RunFailurePolicy runFailurePolicy)
    {
        _httpClient = httpClient;
        _idleWindowCoordinator = idleWindowCoordinator;
        _scaleEvidenceServices = scaleEvidenceServices.ToDictionary(service => service.ProviderId, StringComparer.OrdinalIgnoreCase);
        _parityExceptionRecorder = parityExceptionRecorder;
        _computeStepExecutor = computeStepExecutor;
        _resultRecordFactory = resultRecordFactory;
        _runFailurePolicy = runFailurePolicy;
    }

    public async Task<IReadOnlyList<ResultRecord>> ExecuteAsync(
        string runId,
        IReadOnlyList<ProviderDeployment> providers,
        WorkloadDefinition workload,
        CancellationToken cancellationToken)
    {
        var records = new List<ResultRecord>();

        foreach (var provider in providers)
        {
            foreach (var step in workload.Steps.OrderBy(step => step.Sequence))
            {
                ResultRecord record;
                if (step.Intent == "cold")
                {
                    await _idleWindowCoordinator.WaitForProviderAsync(provider, cancellationToken);
                    var evidence = await ConfirmScaleEvidenceAsync(provider, cancellationToken);
                    record = await ExecuteColdStepAsync(runId, provider, step, evidence, cancellationToken);
                }
                else
                {
                    var payload = workload.PayloadCatalog.Single(payload => payload.PayloadId == step.PayloadRef);
                    record = await _computeStepExecutor.ExecuteAsync(runId, provider, step, payload, cancellationToken);
                }

                records.Add(record);
                if (!_runFailurePolicy.ShouldContinue(record))
                {
                    return records;
                }
            }
        }

        return records;
    }

    private async Task<ResultRecord> ExecuteColdStepAsync(
        string runId,
        ProviderDeployment provider,
        WorkloadStep step,
        ScaleEvidenceResult evidence,
        CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        List<string>? annotationRefs = null;

        if (!evidence.ScaleToZeroConfirmed)
        {
            annotationRefs =
            [
                _parityExceptionRecorder.Record(
                    provider.ProviderId,
                    scope: "step",
                    type: evidence.ExceptionType ?? "parity-exception",
                    description: evidence.Description ?? "Scale-to-zero evidence could not be confirmed.",
                    impact: evidence.Impact ?? "Interpret cold-start latency with care.",
                    relatedStepId: step.StepId)
            ];
        }

        try
        {
            using var response = await _httpClient.GetAsync(new Uri($"{provider.BaseUrl.TrimEnd('/')}{provider.StartupPath}"), cancellationToken);
            stopwatch.Stop();

            return _resultRecordFactory.CreateStartupRecord(
                runId,
                provider,
                step,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed.TotalMilliseconds,
                (int)response.StatusCode,
                evidence.ScaleToZeroConfirmed,
                annotationRefs,
                response.IsSuccessStatusCode ? null : "http-error",
                response.IsSuccessStatusCode ? null : await response.Content.ReadAsStringAsync(cancellationToken));
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return _resultRecordFactory.CreateStartupRecord(
                runId,
                provider,
                step,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed.TotalMilliseconds,
                500,
                evidence.ScaleToZeroConfirmed,
                annotationRefs,
                exception.GetType().Name,
                exception.Message);
        }
    }

    private Task<ScaleEvidenceResult> ConfirmScaleEvidenceAsync(ProviderDeployment provider, CancellationToken cancellationToken)
    {
        if (_scaleEvidenceServices.TryGetValue(provider.ProviderId, out var service))
        {
            return service.ConfirmScaleToZeroAsync(provider, cancellationToken);
        }

        return Task.FromResult(new ScaleEvidenceResult(false, "platform-limitation", "No scale evidence service is registered for this provider.", "Cold-start intent is preserved with a parity caveat."));
    }
}
