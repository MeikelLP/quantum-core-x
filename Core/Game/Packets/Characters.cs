#nullable enable
using QuantumCore.Core.Networking;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets
{
    [Packet(0x20, EDirection.Outgoing)]
    [PacketGenerator]
    public partial class Characters
    {
        [Field(0, ArrayLength = 4)]
        public Character[] CharacterList { get; set; } = new Character[4] {
            new Character(), new Character(), new Character(), new Character()
        };
        [Field(1, ArrayLength = 4)]
        public uint[] GuildIds { get; set; } = new uint[4];
        [Field(2, ArrayLength = 4, Length = 13)]
        public string[] GuildNames { get; set; } = new string[4];
        [Field(3)]
        public uint Unknown1 { get; set; }
        [Field(4)]
        public uint Unknown2 { get; set; }
    }
}