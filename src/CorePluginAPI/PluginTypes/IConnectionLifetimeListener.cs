namespace QuantumCore.API.PluginTypes;

public interface IConnectionLifetimeListener
{
    Task OnConnectedAsync(CancellationToken token);
    Task OnDisconnectedAsync(CancellationToken token);
}