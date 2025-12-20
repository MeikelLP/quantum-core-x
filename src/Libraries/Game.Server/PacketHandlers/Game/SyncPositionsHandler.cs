using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World;
using static QuantumCore.API.Game.Types.Entities.EEntityType;

namespace QuantumCore.Game.PacketHandlers.Game;

public class SyncPositionsHandler : IGamePacketHandler<SyncPositions>
{
    public Task ExecuteAsync(GamePacketContext<SyncPositions> ctx, CancellationToken token = default)
    {
        if (ctx.Connection.Player is not { Map: Map localMap } player)
        {
            return Task.CompletedTask;
        }

        var positions = new List<SyncPositionElement>(ctx.Packet.Positions.Length);
        foreach (var position in ctx.Packet.Positions)
        {
            var entity = localMap.GetEntity(position.Vid);
            if (entity is null || entity.Type is NPC or WARP or GOTO)
            {
                continue;
            }

            // TODO: should we sync on the server as well? or only forward to neighboring players?
            // i.e. entity.Move(position.X, position.Y);
            
            positions.Add(position);
        }

        if (positions.Count == 0)
        {
            return Task.CompletedTask;
        }

        player.SafeBroadcastNearby(new SyncPositionsOut { Positions = positions.ToArray() }, false);

        return Task.CompletedTask;
    }
}
