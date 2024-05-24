using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x88)]
public readonly ref partial struct CharacterInfo
{
    public readonly uint Vid;
    [FixedSizeString(25)] public readonly string Name;
    [FixedSizeArray(4)] public readonly ushort[] Parts;
    public readonly byte Empire;
    public readonly uint GuildId;
    public readonly uint Level;
    public readonly short RankPoints;
    public readonly byte PkMode;
    public readonly uint MountVnum;

    public CharacterInfo(uint vid, string name, ushort[] parts, byte empire, uint guildId, uint level,
        short rankPoints, byte pkMode, uint mountVnum)
    {
        Vid = vid;
        Name = name;
        Parts = parts;
        Empire = empire;
        GuildId = guildId;
        Level = level;
        RankPoints = rankPoints;
        PkMode = pkMode;
        MountVnum = mountVnum;
    }
}