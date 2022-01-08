using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x0e, EDirection.Outgoing)]
    public class CharacterDead
    {
        [Field(0)]
        public uint Vid { get; set; }
    }
}