using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(QuickBarRemove))]
public class QuickBarRemoveHandler
{
    public void Execute(GamePacketContext ctx, QuickBarRemove packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.QuickSlotBar.Remove(packet.Position);
    }
}