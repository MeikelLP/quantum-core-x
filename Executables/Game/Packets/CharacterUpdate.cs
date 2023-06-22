using QuantumCore.Core.Packets;

namespace QuantumCore.Game.Packets
{
    [Packet(0x13, EDirection.Outgoing)]
    public class CharacterUpdate
    {
        [Field(0)]
        public uint Vid { get; set; }
        [Field(1, ArrayLength = 4)]
        public ushort[] Parts { get; set; } = new ushort[4];
        [Field(2)]
        public byte MoveSpeed { get; set; }
        [Field(3)]
        public byte AttackSpeed { get; set; }
        [Field(4)]
        public byte State { get; set; }
        [Field(5, ArrayLength = 2)]
        public uint[] Affects { get; set; } = new uint[2];
        [Field(6)]
        public uint GuildId { get; set; }
        [Field(7)]
        public short RankPoints { get; set; }
        [Field(8)]
        public byte PkMode { get; set; }
        [Field(9)]
        public uint MountVnum { get; set; }
    }
}