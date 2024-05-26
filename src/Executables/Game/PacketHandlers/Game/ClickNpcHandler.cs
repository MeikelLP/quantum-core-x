using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(ClickNpc))]
public class ClickNpcHandler
{
    public void Execute(GamePacketContext ctx, ClickNpc packet)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(packet.Vid);
        if (entity == null)
        {
            ctx.Connection.Close();
            return;
        }

        await GameEventManager.OnNpcClick(entity.EntityClass, player);
    }
}