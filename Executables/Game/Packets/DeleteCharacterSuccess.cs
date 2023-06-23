using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0A, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class DeleteCharacterSuccess
    {
        [Field(0)]
        public byte Slot { get; set; }
    }
}
