using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x05, EDirection.Incoming, Sequence = true)]
    [PacketGenerator]
    public partial class DeleteCharacter
    {
        [Field(0)]
        public byte Slot { get; set; }

        [Field(1, Length = 8)]
        public string Code { get; set; } = "";
    }
}
