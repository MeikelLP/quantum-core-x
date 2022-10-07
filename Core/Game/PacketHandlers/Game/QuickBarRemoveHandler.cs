using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarRemoveHandler : IPacketHandler<QuickBarRemove>
{
    public async Task ExecuteAsync(PacketContext<QuickBarRemove> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        await player.QuickSlotBar.Remove(ctx.Packet.Position);
    }
}