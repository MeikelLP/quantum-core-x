using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x11, EDirection.Outgoing)]
[PacketGenerator]
public partial class CharacterPointUpdate
{
    public uint Vid { get; set; }
    public byte Type { get; set; }
    public long Value { get; set; }
    public long Amount { get; set; }
}
