using Benchmark.Contracts.Payloads;

namespace BenchmarkRunner.Services;

public sealed class WorkloadHashService
{
    public async Task<string> ComputeAsync(string path, CancellationToken cancellationToken)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
        return MatrixHashCalculator.ComputeFileHash(bytes);
    }
}
