using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x09, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class CreateCharacterFailure
    {
        [Field(0)] public byte Error { get; set; }
    }
}