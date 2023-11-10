namespace QuantumCore.API.PluginTypes;

public interface ISingletonPlugin
{
    Task InitializeAsync(CancellationToken token = default);
}