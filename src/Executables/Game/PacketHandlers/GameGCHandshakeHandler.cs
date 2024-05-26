using QuantumCore.API.Core.Models;
using QuantumCore.Core.Packets;

namespace QuantumCore.Game.PacketHandlers;

[PacketHandler(typeof(GCHandshake))]
public class GameGCHandshakeHandler
{
    public void Execute(GamePacketContext ctx, GCHandshake packet)
    {
        ctx.Connection.HandleHandshake(new GCHandshakeData
        {
            Delta = packet.Delta,
            Handshake = packet.Handshake,
            Time = packet.Time
        });
    }
}