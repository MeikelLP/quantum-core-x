using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x71, EDirection.Outgoing)]
    public class CharacterDetails
    {
        [Field(0)]
        public uint Vid { get; set; }
        [Field(1)]
        public ushort Class { get; set; }
        [Field(2, Length = 25)]
        public string Name { get; set; }
        [Field(3)]
        public int PositionX { get; set; }
        [Field(4)]
        public int PositionY { get; set; }
        [Field(5)]
        public int PositionZ { get; set; }
        [Field(6)]
        public byte Empire { get; set; }
        [Field(7)]
        public byte SkillGroup { get; set; }
    }
}