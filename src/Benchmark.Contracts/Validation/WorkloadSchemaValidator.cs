using Benchmark.Contracts.Payloads;
using Benchmark.Contracts.Workloads;

namespace Benchmark.Contracts.Validation;

public sealed class WorkloadSchemaValidator
{
    private static readonly string[] CanonicalPayloadIds = ["matrix-100x100", "matrix-200x200"];

    public IReadOnlyList<string> Validate(WorkloadDefinition workload)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(workload.WorkloadVersion))
        {
            errors.Add("workloadVersion is required.");
        }

        if (string.IsNullOrWhiteSpace(workload.Description))
        {
            errors.Add("description is required.");
        }

        if (workload.PayloadCatalog.Count != 2)
        {
            errors.Add("payloadCatalog must contain exactly two payload definitions.");
        }

        var payloadIds = workload.PayloadCatalog.Select(payload => payload.PayloadId).OrderBy(id => id).ToArray();
        if (!payloadIds.SequenceEqual(CanonicalPayloadIds.OrderBy(id => id)))
        {
            errors.Add("payloadCatalog must contain exactly matrix-100x100 and matrix-200x200.");
        }

        foreach (var payload in workload.PayloadCatalog)
        {
            if (payload.Dimension is not (100 or 200))
            {
                errors.Add($"payload {payload.PayloadId} dimension must be 100 or 200.");
            }

            if (payload.LeftMatrix.Length != payload.Dimension || payload.RightMatrix.Length != payload.Dimension)
            {
                errors.Add($"payload {payload.PayloadId} matrices must be square and match dimension.");
            }

            if (payload.LeftMatrix.Any(row => row.Length != payload.Dimension) ||
                payload.RightMatrix.Any(row => row.Length != payload.Dimension))
            {
                errors.Add($"payload {payload.PayloadId} rows must match dimension.");
            }

            var computedHash = MatrixHashCalculator.ComputePayloadHash(payload);
            if (!string.Equals(computedHash, payload.ContentHash, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"payload {payload.PayloadId} contentHash does not match canonical content.");
            }
        }

        if (workload.Steps.Count == 0)
        {
            errors.Add("steps must contain at least one item.");
            return errors;
        }

        var duplicateStepIds = workload.Steps.GroupBy(step => step.StepId).Where(group => group.Count() > 1).Select(group => group.Key);
        foreach (var duplicateStepId in duplicateStepIds)
        {
            errors.Add($"stepId {duplicateStepId} must be unique.");
        }

        var expectedSequence = 1;
        foreach (var step in workload.Steps.OrderBy(step => step.Sequence))
        {
            if (step.Sequence != expectedSequence)
            {
                errors.Add("step sequences must be contiguous starting at 1.");
                break;
            }

            expectedSequence++;
        }

        if (!workload.Steps.Any(step => step.Intent == "cold"))
        {
            errors.Add("steps must include at least one cold step.");
        }

        if (!workload.Steps.Any(step => step.Intent == "warm"))
        {
            errors.Add("steps must include at least one warm step.");
        }

        foreach (var step in workload.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.StepId))
            {
                errors.Add("every step requires a stepId.");
            }

            if (step.Intent == "cold")
            {
                if (step.Endpoint != "startup" || step.Method != "GET")
                {
                    errors.Add($"cold step {step.StepId} must target GET startup.");
                }

                if (!string.IsNullOrWhiteSpace(step.PayloadRef))
                {
                    errors.Add($"cold step {step.StepId} must not reference a payload.");
                }
            }

            if (step.Intent == "warm")
            {
                if (step.Endpoint != "compute" || step.Method != "POST")
                {
                    errors.Add($"warm step {step.StepId} must target POST compute.");
                }

                if (string.IsNullOrWhiteSpace(step.PayloadRef))
                {
                    errors.Add($"warm step {step.StepId} must reference a payload.");
                }
            }

            if (!string.IsNullOrWhiteSpace(step.PayloadRef) &&
                workload.PayloadCatalog.All(payload => payload.PayloadId != step.PayloadRef))
            {
                errors.Add($"step {step.StepId} references an unknown payload.");
            }
        }

        return errors;
    }

    public void EnsureValid(WorkloadDefinition workload)
    {
        var errors = Validate(workload);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
        }
    }
}
