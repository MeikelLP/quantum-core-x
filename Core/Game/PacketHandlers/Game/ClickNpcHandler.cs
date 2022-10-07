using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ClickNpcHandler : IPacketHandler<ClickNpc>
{
    public async Task ExecuteAsync(PacketContext<ClickNpc> ctx, CancellationToken token = default)
    { 
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map.GetEntity(ctx.Packet.Vid);
        if (entity == null)
        {
            ctx.Connection.Close();
            return;
        }
            
        await GameEventManager.OnNpcClick(entity.EntityClass, player);
    }
}