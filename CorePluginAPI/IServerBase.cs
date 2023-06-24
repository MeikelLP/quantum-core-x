using System.Threading.Tasks;

namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnection(IConnection connection);
    Task CallListener(IConnection connection, object packet);
    long ServerTime { get; }
    void CallConnectionListener(IConnection connection);
}