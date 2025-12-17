using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarSwapHandler : IGamePacketHandler<QuickBarSwap>
{
    public Task ExecuteAsync(GamePacketContext<QuickBarSwap> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.QuickSlotBar.Swap(ctx.Packet.Position1, ctx.Packet.Position2);
        return Task.CompletedTask;
    }
}
