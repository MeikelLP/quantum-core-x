namespace QuantumCore.PluginTypes;

public interface IGameTickListener
{
    Task PreUpdateAsync(CancellationToken token);
    Task PostUpdateAsync(CancellationToken token);
}