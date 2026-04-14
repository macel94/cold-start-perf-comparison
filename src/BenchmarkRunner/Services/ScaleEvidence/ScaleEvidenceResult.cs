namespace BenchmarkRunner.Services.ScaleEvidence;

public sealed record ScaleEvidenceResult(
    bool ScaleToZeroConfirmed,
    string? ExceptionType = null,
    string? Description = null,
    string? Impact = null);
