namespace QuantumCore.PluginTypes;

public interface ISingletonPlugin
{
    Task InitializeAsync(CancellationToken token = default);
}