namespace QuantumCore.API;

public interface ILoadable
{
    Task LoadAsync(CancellationToken token = default);
}