using System.Text.Json;
using Benchmark.Contracts.Validation;
using Benchmark.Contracts.Workloads;

namespace BenchmarkRunner.Services;

public sealed class WorkloadFileLoader
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly WorkloadSchemaValidator _validator;

    public WorkloadFileLoader(WorkloadSchemaValidator validator)
    {
        _validator = validator;
    }

    public async Task<WorkloadDefinition> LoadAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = File.OpenRead(path);
        var workload = await JsonSerializer.DeserializeAsync<WorkloadDefinition>(stream, SerializerOptions, cancellationToken);
        if (workload is null)
        {
            throw new InvalidOperationException("Workload file could not be deserialized.");
        }

        _validator.EnsureValid(workload);
        return workload;
    }
}
