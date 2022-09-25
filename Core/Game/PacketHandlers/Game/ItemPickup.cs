using System.Threading;
using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemPickupHandler : IPacketHandler<ItemPickup>
{
    public async Task ExecuteAsync(PacketContext<ItemPickup> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map.GetEntity(ctx.Packet.Vid);
        if (entity is not GroundItem groundItem)
        {
            // we can only pick up ground items
            return;
        }

        await player.Pickup(groundItem);
    }
}