using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

public class QuickBarAddHandler : IGamePacketHandler<QuickBarAdd>
{
    public async Task ExecuteAsync(GamePacketContext<QuickBarAdd> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        await player.QuickSlotBar.Add(ctx.Packet.Position, new QuickSlotData {
            Position = ctx.Packet.Slot.Position,
            Type = ctx.Packet.Slot.Position
        });
    }
}