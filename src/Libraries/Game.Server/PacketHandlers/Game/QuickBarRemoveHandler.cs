using QuantumCore.API;
using QuantumCore.API.Packets.QuickBar;
using QuantumCore.API.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarRemoveHandler : IGamePacketHandler<QuickBarRemove>
{
    public Task ExecuteAsync(GamePacketContext<QuickBarRemove> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.QuickSlotBar.Remove(ctx.Packet.Position);
        return Task.CompletedTask;
    }
}
