using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.API.PluginTypes;

public interface IPacketHandler
{
}

public interface IGamePacketHandler<T> : IPacketHandler where T : class
{
    Task ExecuteAsync(GamePacketContext<T> ctx, CancellationToken token = default);
}

public interface IAuthPacketHandler<T> : IPacketHandler where T : class
{
    Task ExecuteAsync(AuthPacketContext<T> ctx, CancellationToken token = default);
}