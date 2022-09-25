using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class ItemGiveHandler : IPacketHandler<ItemGive>
{
    private readonly ILogger<ItemGiveHandler> _logger;

    public ItemGiveHandler(ILogger<ItemGiveHandler> logger)
    {
        _logger = logger;
    }
        
    public async Task ExecuteAsync(PacketContext<ItemGive> ctx, CancellationToken token = default)
    {
        var player = ctx.Connection.Player;
        if (player == null)
        {
            ctx.Connection.Close();
            return;
        }

        var entity = player.Map.GetEntity(ctx.Packet.TargetVid);
        if (entity == null)
        {
            _logger.LogDebug("Ignore item give to non existing entity");
            return;
        }

        var item = player.GetItem(ctx.Packet.Window, ctx.Packet.Position);
        if (item == null)
        {
            return;
        }
            
        _logger.LogInformation("Item give to {Entity}", entity);
        await GameEventManager.OnNpcGive(entity.EntityClass, player, item);
    }
}