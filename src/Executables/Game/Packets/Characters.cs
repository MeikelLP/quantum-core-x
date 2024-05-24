using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x20)]
public partial class Characters
{
    [FixedSizeArray(4)] public Character[] CharacterList { get; set; } = [];
    [FixedSizeArray(4)] public uint[] GuildIds { get; set; } = [];

    [FixedSizeArray(4)]
    [FixedSizeString(13)]
    public string[] GuildNames { get; set; } = [];

    public uint Unknown1 { get; set; }
    public uint Unknown2 { get; set; }
}