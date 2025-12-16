using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ClickNpcHandler : IGamePacketHandler<ClickNpc>
{
    public async Task ExecuteAsync(GamePacketContext<ClickNpc> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player is null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map?.GetEntity(ctx.Packet.Vid);
        if (entity is null)
        {
            ctx.Connection.Close();
            return;
        }

        await GameEventManager.OnNpcClick(entity.EntityClass, player);
    }
}
