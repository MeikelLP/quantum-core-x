using QuantumCore.API.Core.Models;
using QuantumCore.Core.Packets;

namespace QuantumCore.Auth.PacketHandlers;

public class GCHandshakeHandler : IAuthPacketHandler<GCHandshake>
{
    public ValueTask ExecuteAsync(AuthPacketContext<GCHandshake> context, CancellationToken token = default)
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