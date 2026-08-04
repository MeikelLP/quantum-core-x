using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x0F, EDirection.INCOMING, Sequence = true)]
[PacketGenerator]
public partial class ItemPickup
{
    [Field(0)] public uint Vid { get; set; }
}
