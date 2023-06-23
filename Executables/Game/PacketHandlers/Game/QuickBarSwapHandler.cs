using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarSwapHandler : IGamePacketHandler<QuickBarSwap>
{
    public async Task ExecuteAsync(GamePacketContext<QuickBarSwap> ctx, CancellationToken token = default)
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