namespace QuantumCore;

public interface ILoadable
{
    Task LoadAsync(CancellationToken token = default);
}