using System.Threading.Tasks;
using QuantumCore.Networking;

namespace QuantumCore.API;

public interface IServerBase
{
    Task RemoveConnection(IConnection connection);
    Task CallListener(IConnection connection, IPacketSerializable packet);
    long ServerTime { get; }
    void CallConnectionListener(IConnection connection);
}