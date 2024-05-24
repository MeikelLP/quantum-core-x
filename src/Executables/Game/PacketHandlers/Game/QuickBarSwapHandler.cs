using QuantumCore.Game.Packets.QuickBar;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(QuickBarSwap))]
public class QuickBarSwapHandler
{
    public void Execute(GamePacketContext ctx, QuickBarSwap packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        player.QuickSlotBar.Swap(packet.Position1, packet.Position2);
    }
}