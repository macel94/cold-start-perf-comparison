using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Benchmark.Contracts.Providers;
using Benchmark.Contracts.Results;
using Benchmark.Contracts.Workloads;
using BenchmarkApp.Models;

namespace BenchmarkRunner.Services;

public sealed class ComputeStepExecutor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly MatrixResultVerifier _resultVerifier;
    private readonly ResultRecordFactory _resultRecordFactory;

    public ComputeStepExecutor(HttpClient httpClient, MatrixResultVerifier resultVerifier, ResultRecordFactory resultRecordFactory)
    {
        _httpClient = httpClient;
        _resultVerifier = resultVerifier;
        _resultRecordFactory = resultRecordFactory;
    }

    public async Task<ResultRecord> ExecuteAsync(string runId, ProviderDeployment provider, WorkloadStep step, PayloadDefinition payload, CancellationToken cancellationToken)
    {
        var request = new MatrixComputeRequest(payload.PayloadId, payload.Dimension, payload.LeftMatrix, payload.RightMatrix);
        var startedAtUtc = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var response = await _httpClient.PostAsync(
                BuildUri(provider.BaseUrl, provider.ComputePath),
                new StringContent(JsonSerializer.Serialize(request, SerializerOptions), Encoding.UTF8, "application/json"),
                cancellationToken);

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            stopwatch.Stop();

            MatrixComputeResponse? body = null;
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    body = JsonSerializer.Deserialize<MatrixComputeResponse>(content, SerializerOptions);
                }
                catch (JsonException)
                {
                    // Keep body null and treat as a failed correctness check.
                }
            }

            var correctness = body is not null && _resultVerifier.Verify(payload, body) ? "passed" : "failed";
            return _resultRecordFactory.CreateComputeRecord(
                runId,
                provider,
                step,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed.TotalMilliseconds,
                (int)response.StatusCode,
                correctness,
                body,
                response.IsSuccessStatusCode ? null : "http-error",
                response.IsSuccessStatusCode ? null : content);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            return _resultRecordFactory.CreateComputeRecord(
                runId,
                provider,
                step,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                stopwatch.Elapsed.TotalMilliseconds,
                500,
                "failed",
                responseBody: null,
                errorType: exception.GetType().Name,
                errorDetail: exception.Message);
        }
    }

    private static Uri BuildUri(string baseUrl, string path) => new($"{baseUrl.TrimEnd('/')}{path}");
}
