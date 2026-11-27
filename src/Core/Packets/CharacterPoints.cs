using QuantumCore.Networking;

namespace QuantumCore.API.Packets;

[Packet(0x10, EDirection.OUTGOING)]
[PacketGenerator]
public partial class CharacterPoints
{
    [Field(0, ArrayLength = 255)] public uint[] Points { get; set; } = new uint[255];
}
