using System.Threading;
using System.Threading.Tasks;

namespace QuantumCore.Core.Networking;

public interface IPacketHandler
{
}
public interface IPacketHandler<T> : IPacketHandler where T : class
{
    Task ExecuteAsync(PacketContext<T> ctx, CancellationToken token = default);
}