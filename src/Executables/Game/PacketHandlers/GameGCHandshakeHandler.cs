using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.PacketHandlers;

public class GameGCHandshakeHandler : IGamePacketHandler<GCHandshake>
{
    public Task ExecuteAsync(GamePacketContext<GCHandshake> ctx, CancellationToken token = default)
    {
        ctx.Connection.HandleHandshake(new GCHandshakeData {
            Delta = ctx.Packet.Delta,
            Handshake = ctx.Packet.Handshake,
            Time = ctx.Packet.Time
        });
        return Task.CompletedTask;
    }
}
