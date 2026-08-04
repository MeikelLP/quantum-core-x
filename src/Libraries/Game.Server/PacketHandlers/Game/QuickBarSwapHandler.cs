using QuantumCore.API;
using QuantumCore.API.Packets.QuickBar;
using QuantumCore.API.PluginTypes;

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
