using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarSwapHandler : IPacketHandler<QuickBarSwap>
{
    public async Task ExecuteAsync(PacketContext<QuickBarSwap> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        await player.QuickSlotBar.Swap(ctx.Packet.Position1, ctx.Packet.Position2);
    }
}