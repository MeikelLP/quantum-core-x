namespace QuantumCore.PluginTypes;

public interface IConnectionLifetimeListener
{
    Task OnConnectedAsync(CancellationToken token);
    Task OnDisconnectedAsync(CancellationToken token);
}