using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class GameTickListener : IGameTickListener
{
    static GameTickListener()
    {
        Metrics.CreateHistogram("world_update_duration_seconds", "How long did a world update took");
    }

    public Task PreUpdateAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task PostUpdateAsync(CancellationToken token)
    {
        return Task.CompletedTask;
    }
}