using Benchmark.Contracts.Providers;

namespace BenchmarkRunner.Services;

public sealed class ColdStartIdleWindowCoordinator
{
    private readonly Func<TimeSpan, CancellationToken, Task> _delay;

    public ColdStartIdleWindowCoordinator(Func<TimeSpan, CancellationToken, Task>? delay = null)
    {
        _delay = delay ?? Task.Delay;
    }

    public Task WaitForProviderAsync(ProviderDeployment provider, CancellationToken cancellationToken)
    {
        var idleWindow = TimeSpan.FromMinutes(provider.IdleWindowMinutes);
        return _delay(idleWindow, cancellationToken);
    }
}
