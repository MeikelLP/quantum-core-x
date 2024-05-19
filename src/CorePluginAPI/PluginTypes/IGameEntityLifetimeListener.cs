namespace QuantumCore.API.PluginTypes;

public interface IGameEntityLifetimeListener
{
    Task OnPreCreatedAsync(CancellationToken token = default);
    Task OnPostCreatedAsync(CancellationToken token = default);
    Task OnPreDeletedAsync(CancellationToken token = default);
    Task OnPostDeletedAsync(CancellationToken token = default);
}