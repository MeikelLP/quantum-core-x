using Prometheus;
using QuantumCore.API.PluginTypes;

namespace PrometheusPlugin;

public class ClientConnectedListener : IConnectionLifetimeListener
{
    private readonly Gauge _openConnections = Metrics.CreateGauge("open_connections", "Currently open connections");
    
    public Task OnConnectedAsync(CancellationToken token)
    {
        _openConnections.Inc();
        
        return Task.CompletedTask;
    }

    public Task OnDisconnectedAsync(CancellationToken token)
    {
        _openConnections.Dec();
        
        return Task.CompletedTask;
    }
}