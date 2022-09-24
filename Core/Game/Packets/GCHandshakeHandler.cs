using System.Threading;
using System.Threading.Tasks;
using QuantumCore.Core.Networking;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets;

public class GCHandshakeHandler : IPacketHandler<GCHandshake>
{
    public async Task ExecuteAsync(PacketContext<GCHandshake> ctx, CancellationToken token = default)
    {
        await ctx.Connection.HandleHandshake(ctx.Packet);
    }
}