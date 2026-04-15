using Benchmark.Contracts.Results;

namespace BenchmarkRunner.Services;

public sealed class SummaryMetricCalculator
{
    public IReadOnlyList<SummaryMetric> Calculate(IReadOnlyList<ResultRecord> records, IReadOnlyList<ParityException> parityExceptions)
    {
        return records
            .GroupBy(record => new { record.ProviderId, record.Intent })
            .OrderBy(group => group.Key.ProviderId)
            .ThenBy(group => group.Key.Intent)
            .Select(group =>
            {
                var latencies = group.Select(record => record.LatencyMs).OrderBy(value => value).ToArray();
                var relatedParityCount = parityExceptions.Count(exception =>
                    exception.ProviderId == group.Key.ProviderId &&
                    (exception.Scope == "provider" || group.Any(record => record.StepId == exception.RelatedStepId)));

                return new SummaryMetric
                {
                    ProviderId = group.Key.ProviderId,
                    Intent = group.Key.Intent,
                    SampleCount = latencies.Length,
                    MinLatencyMs = latencies.Min(),
                    P50LatencyMs = Percentile(latencies, 0.50d),
                    P95LatencyMs = Percentile(latencies, 0.95d),
                    P99LatencyMs = Percentile(latencies, 0.99d),
                    MaxLatencyMs = latencies.Max(),
                    ErrorCount = group.Count(record => record.ErrorType is not null || record.HttpStatus >= 400),
                    ParityExceptionCount = relatedParityCount
                };
            })
            .ToList();
    }

    private static double Percentile(double[] orderedValues, double percentile)
    {
        if (orderedValues.Length == 0)
        {
            return 0;
        }

        var index = Math.Max(0, (int)Math.Ceiling(orderedValues.Length * percentile) - 1);
        return orderedValues[index];
    }
}
