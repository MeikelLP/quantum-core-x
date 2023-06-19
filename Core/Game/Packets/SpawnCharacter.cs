using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x01, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class SpawnCharacter
    {
        [Field(0)]
        public uint Vid { get; set; }
        [Field(1)]
        public float Angle { get; set; }
        [Field(2)]
        public int PositionX { get; set; }
        [Field(3)]
        public int PositionY { get; set; }
        [Field(4)]
        public int PositionZ { get; set; }
        [Field(5)]
        public byte CharacterType { get; set; }
        [Field(6)]
        public ushort Class { get; set; }
        [Field(7)]
        public byte MoveSpeed { get; set; }
        [Field(8)]
        public byte AttackSpeed { get; set; }
        [Field(9)]
        public byte State { get; set; }
        [Field(10, ArrayLength = 2)]
        public uint[] Affects { get; set; } = new uint[2];
    }
}