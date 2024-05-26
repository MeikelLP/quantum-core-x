using QuantumCore.API.Core.Models;
using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(QuickBarAdd))]
public class QuickBarAddHandler
{
    public void Execute(GamePacketContext ctx, QuickBarAdd packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.QuickSlotBar.Add(packet.Position, new QuickSlotData
        {
            Position = packet.Slot.Position,
            Type = packet.Slot.Position
        });
    }
}