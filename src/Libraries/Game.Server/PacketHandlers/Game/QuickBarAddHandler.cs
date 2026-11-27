using QuantumCore.Packets.QuickBar;
using QuantumCore.Models;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarAddHandler : IGamePacketHandler<QuickBarAdd>
{
    public Task ExecuteAsync(GamePacketContext<QuickBarAdd> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        player.QuickSlotBar.Add(ctx.Packet.Position,
            new QuickSlotData { Position = ctx.Packet.Slot.Position, Type = ctx.Packet.Slot.Type });
        return Task.CompletedTask;
    }
}
