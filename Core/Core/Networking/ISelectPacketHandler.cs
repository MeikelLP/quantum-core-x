using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;

namespace QuantumCore.Core.Networking;

internal interface ISelectPacketHandler<T> : IPacketHandler where T : class
{
    Task ExecuteAsync(PacketContext<T> ctx, CancellationToken token = default);
}