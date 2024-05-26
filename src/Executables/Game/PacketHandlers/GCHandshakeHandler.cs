using QuantumCore.API.Core.Models;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.PacketHandlers;

[PacketHandler(typeof(GCHandshake))]
public class GCHandshakeHandler : IGamePacketHandler<GCHandshake>
{
    public ValueTask ExecuteAsync(GamePacketContext<GCHandshake> context, CancellationToken token = default)
    {
        context.Connection.HandleHandshake(new GCHandshakeData
        {
            Delta = context.Packet.Delta,
            Handshake = context.Packet.Handshake,
            Time = context.Packet.Time
        });

        return ValueTask.CompletedTask;
    }
}