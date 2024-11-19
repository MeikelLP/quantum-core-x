using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Guild;

[Packet(0x50, EDirection.Incoming, Sequence = true)]
[SubPacket(0x06, 0)]
[PacketGenerator]
public partial class GuildNewsDelete
{
    [Field(0)] public uint Id { get; set; }
}