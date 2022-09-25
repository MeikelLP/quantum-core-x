using System.Threading.Tasks;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IGameServer
{
    IWorld World { get; }
    long ServerTime { get; }
    Task CallListener(IConnection connection, object packet);
    Task RemoveConnection(IConnection connection);
}