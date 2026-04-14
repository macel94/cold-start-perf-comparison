using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using Benchmark.Contracts.Validation;
using BenchmarkRunner.Configuration;
using BenchmarkRunner.Services;
using BenchmarkRunner.Services.ScaleEvidence;

var cancellationToken = CancellationToken.None;
var parsedArguments = ParseArguments(args);

var workloadPath = parsedArguments.TryGetValue("workload", out var workloadValue)
    ? workloadValue
    : Path.Combine("workloads", "v1", "cross-cloud-sequential.json");

var outputPath = parsedArguments.TryGetValue("output", out var outputValue)
    ? outputValue
    : Path.Combine("benchmark-results", $"run-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.json");

var providerIds = parsedArguments.TryGetValue("providers", out var providersValue)
    ? providersValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    : Array.Empty<string>();

var networkLocationLabel = parsedArguments.GetValueOrDefault("network-location-label");

var providerOptions = LoadProviderOptions(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"));
var providerCatalog = new ProviderTargetCatalog();
var selectedProviders = providerCatalog.ResolveTargets(providerOptions, providerIds);

var workloadValidator = new WorkloadSchemaValidator();
var resultsValidator = new ResultsSchemaValidator();
var workloadLoader = new WorkloadFileLoader(workloadValidator);
var workloadHashService = new WorkloadHashService();
var runEnvelopeBuilder = new RunEnvelopeBuilder();
var resultEnvelopeWriter = new ResultEnvelopeWriter();
var summaryMetricCalculator = new SummaryMetricCalculator();
var resultRecordFactory = new ResultRecordFactory();
var runFailurePolicy = new RunFailurePolicy();
var parityExceptionRecorder = new ParityExceptionRecorder();
var idleWindowCoordinator = new ColdStartIdleWindowCoordinator();
var matrixResultVerifier = new MatrixResultVerifier();

using var httpClient = new HttpClient();
var computeStepExecutor = new ComputeStepExecutor(httpClient, matrixResultVerifier, resultRecordFactory);
var scaleEvidenceServices = new IProviderScaleEvidenceService[]
{
    new CloudRunScaleEvidenceService(),
    new AwsLambdaScaleEvidenceService(),
    new AzureContainerAppsScaleEvidenceService(),
    new ScalewayScaleEvidenceService()
};

var sequentialWorkloadExecutor = new SequentialWorkloadExecutor(
    httpClient,
    idleWindowCoordinator,
    scaleEvidenceServices,
    parityExceptionRecorder,
    computeStepExecutor,
    resultRecordFactory,
    runFailurePolicy);

var workload = await workloadLoader.LoadAsync(workloadPath, cancellationToken);
var workloadHash = await workloadHashService.ComputeAsync(workloadPath, cancellationToken);
var runnerVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

var startedRun = runEnvelopeBuilder.CreateStartedRun(
    workload,
    selectedProviders,
    workloadHash,
    apiContractVersion: "1.0.0",
    resultSchemaVersion: "1.0.0",
    runnerVersion,
    networkLocationLabel);

var records = await sequentialWorkloadExecutor.ExecuteAsync(startedRun.RunId, selectedProviders, workload, cancellationToken);
var summaryMetrics = summaryMetricCalculator.Calculate(records, parityExceptionRecorder.Exceptions);
var completedRun = runEnvelopeBuilder.Complete(
    startedRun,
    records,
    parityExceptionRecorder.Exceptions,
    summaryMetrics,
    runFailurePolicy.DetermineStatus(records, parityExceptionRecorder.Exceptions));

resultsValidator.EnsureValid(completedRun);
await resultEnvelopeWriter.WriteAsync(completedRun, outputPath, cancellationToken);

Console.WriteLine($"Benchmark run completed with status '{completedRun.Status}'.");
Console.WriteLine($"Results written to {outputPath}.");

static Dictionary<string, string> ParseArguments(string[] args)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    for (var index = 0; index < args.Length; index++)
    {
        var argument = args[index];
        if (!argument.StartsWith("--", StringComparison.Ordinal))
        {
            continue;
        }

        var key = argument[2..];
        var hasValue = index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal);
        result[key] = hasValue ? args[++index] : "true";
    }

    return result;
}

static ProviderTargetOptions LoadProviderOptions(string path)
{
    var document = JsonDocument.Parse(File.ReadAllText(path));
    if (!document.RootElement.TryGetProperty(ProviderTargetOptions.SectionName, out var section))
    {
        return new ProviderTargetOptions();
    }

    return section.Deserialize<ProviderTargetOptions>(new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? new ProviderTargetOptions();
}
