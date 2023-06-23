using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0e, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class CharacterDead
    {
        [Field(0)]
        public uint Vid { get; set; }
    }
}