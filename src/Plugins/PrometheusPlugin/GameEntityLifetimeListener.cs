using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class GameEntityLifetimeListener : IGameEntityLifetimeListener
{
    private readonly Gauge _entities = Metrics.CreateGauge("entities", "Currently handles entities");
    
    public Task OnPreCreatedAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task OnPostCreatedAsync(CancellationToken token = default)
    {
        _entities.Inc();
        
        return Task.CompletedTask;
    }

    public Task OnPreDeletedAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public Task OnPostDeletedAsync(CancellationToken token = default)
    {
        _entities.Dec();
        
        return Task.CompletedTask;
    }
}