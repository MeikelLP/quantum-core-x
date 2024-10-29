using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x02, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class RemoveCharacter
    {
        [Field(0)] public uint Vid { get; set; }
    }
}