using Benchmark.Contracts.Results;

namespace BenchmarkRunner.Services;

public sealed class ParityExceptionRecorder
{
    private readonly List<ParityException> _exceptions = [];

    public IReadOnlyList<ParityException> Exceptions => _exceptions;

    public string Record(string providerId, string scope, string type, string description, string impact, string? relatedStepId = null)
    {
        var exceptionId = $"px-{providerId}-{_exceptions.Count + 1:D3}";
        _exceptions.Add(new ParityException
        {
            ParityExceptionId = exceptionId,
            ProviderId = providerId,
            Scope = scope,
            Type = type,
            Description = description,
            Impact = impact,
            RelatedStepId = relatedStepId,
            RecordedAtUtc = DateTimeOffset.UtcNow
        });

        return exceptionId;
    }
}
