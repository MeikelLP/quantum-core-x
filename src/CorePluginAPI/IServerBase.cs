namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnection(IConnection connection);
    long ServerTime { get; }
    void CallConnectionListener(IConnection connection);
}