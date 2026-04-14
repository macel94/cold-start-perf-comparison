using Benchmark.Contracts.Results;

namespace Benchmark.Contracts.Validation;

public sealed class ResultsSchemaValidator
{
    public IReadOnlyList<string> Validate(BenchmarkRun run)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(run.RunId))
        {
            errors.Add("runId is required.");
        }

        if (run.StartedAtUtc == default)
        {
            errors.Add("startedAtUtc is required.");
        }

        if (string.IsNullOrWhiteSpace(run.WorkloadVersion) ||
            string.IsNullOrWhiteSpace(run.WorkloadFileHash) ||
            string.IsNullOrWhiteSpace(run.ApiContractVersion) ||
            string.IsNullOrWhiteSpace(run.ResultSchemaVersion) ||
            string.IsNullOrWhiteSpace(run.RunnerVersion))
        {
            errors.Add("run metadata fields must be populated.");
        }

        foreach (var record in run.Records)
        {
            if (record.LatencyMs < 0)
            {
                errors.Add($"record {record.StepId} latency must be non-negative.");
            }

            if (record.Intent == "cold")
            {
                if (record.Endpoint != "startup")
                {
                    errors.Add($"cold record {record.StepId} must target startup.");
                }

                if (record.ScaleToZeroConfirmed is null)
                {
                    errors.Add($"cold record {record.StepId} must set scaleToZeroConfirmed.");
                }

                if (record.ScaleToZeroConfirmed == false && (record.AnnotationRefs is null || record.AnnotationRefs.Count == 0))
                {
                    errors.Add($"cold record {record.StepId} requires annotationRefs when scaleToZeroConfirmed is false.");
                }

                if (record.Correctness != "not-applicable")
                {
                    errors.Add($"cold record {record.StepId} correctness must be not-applicable.");
                }
            }

            if (record.Intent == "warm")
            {
                if (record.Endpoint != "compute")
                {
                    errors.Add($"warm record {record.StepId} must target compute.");
                }

                if (record.ScaleToZeroConfirmed is not null)
                {
                    errors.Add($"warm record {record.StepId} must omit scaleToZeroConfirmed.");
                }

                if (record.HttpStatus < 400 && record.ResponseBody is null)
                {
                    errors.Add($"successful warm record {record.StepId} must include responseBody.");
                }
            }
        }

        foreach (var summary in run.SummaryMetrics)
        {
            if (summary.SampleCount < 0 || summary.ErrorCount < 0 || summary.ParityExceptionCount < 0)
            {
                errors.Add($"summary for {summary.ProviderId}/{summary.Intent} contains negative counts.");
            }

            if (summary.MinLatencyMs > summary.MaxLatencyMs)
            {
                errors.Add($"summary for {summary.ProviderId}/{summary.Intent} has min greater than max.");
            }
        }

        return errors;
    }

    public void EnsureValid(BenchmarkRun run)
    {
        var errors = Validate(run);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
    }
}
