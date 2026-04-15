using Benchmark.Contracts.Results;

namespace BenchmarkRunner.Services;

public sealed class RunFailurePolicy
{
    public bool ShouldContinue(ResultRecord record) => true;

    public string DetermineStatus(IReadOnlyList<ResultRecord> records, IReadOnlyList<ParityException> parityExceptions)
    {
        if (records.Count == 0)
        {
            return "failed";
        }

        return records.Any(record => record.ErrorType is not null || record.HttpStatus >= 400) || parityExceptions.Count > 0
            ? "completed-with-errors"
            : "completed";
    }
}
