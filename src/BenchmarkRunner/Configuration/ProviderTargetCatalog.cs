using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Configuration;

public sealed class ProviderTargetCatalog
{
    public IReadOnlyList<ProviderDeployment> ResolveTargets(ProviderTargetOptions options, IEnumerable<string>? requestedProviderIds = null)
    {
        var providers = options.Providers
            .Where(provider => !string.IsNullOrWhiteSpace(provider.ProviderId))
            .ToDictionary(provider => provider.ProviderId, StringComparer.OrdinalIgnoreCase);

        if (providers.Count == 0)
        {
            throw new InvalidOperationException("No provider targets are configured.");
        }

        if (requestedProviderIds is null || !requestedProviderIds.Any())
        {
            return providers.Values.ToList();
        }

        var results = new List<ProviderDeployment>();
        foreach (var providerId in requestedProviderIds)
        {
            if (!providers.TryGetValue(providerId, out var provider))
            {
                throw new InvalidOperationException($"Unknown provider target '{providerId}'.");
            }

            results.Add(provider);
        }

        return results;
    }
}
