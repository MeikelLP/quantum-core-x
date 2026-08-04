using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x08, EDirection.OUTGOING)]
[PacketGenerator]
public partial class CreateCharacterSuccess
{
    [Field(0)] public byte Slot { get; set; }
    [Field(1)] public Character Character { get; set; } = new Character();
}
