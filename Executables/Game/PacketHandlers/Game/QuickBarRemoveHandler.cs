using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarRemoveHandler : IGamePacketHandler<QuickBarRemove>
{
    public Task ExecuteAsync(GamePacketContext<QuickBarRemove> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.QuickSlotBar.Remove(ctx.Packet.Position);
        return Task.CompletedTask;
    }
}
