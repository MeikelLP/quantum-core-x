using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x88, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class CharacterInfo
    {
        [Field(0)]
        public uint Vid { get; set; }
        [Field(1, Length = 25)]
        public string Name { get; set; }
        [Field(2, ArrayLength = 4)]
        public ushort[] Parts { get; set; } = new ushort[4];
        [Field(3)]
        public byte Empire { get; set; }
        [Field(4)]
        public uint GuildId { get; set; }
        [Field(5)]
        public uint Level { get; set; }
        [Field(6)]
        public short RankPoints { get; set; }
        [Field(7)]
        public byte PkMode { get; set; }
        [Field(8)]
        public uint MountVnum { get; set; }
    }
}