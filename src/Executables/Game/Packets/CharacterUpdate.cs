using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x13)]
public readonly ref partial struct CharacterUpdate
{
    public readonly uint Vid;
    [FixedSizeArray(4)] public readonly ushort[] Parts;
    public readonly byte MoveSpeed;
    public readonly byte AttackSpeed;
    public readonly byte State;
    [FixedSizeArray(2)] public readonly uint[] Affects;
    public readonly uint GuildId;
    public readonly short RankPoints;
    public readonly byte PkMode;
    public readonly uint MountVnum;

    public CharacterUpdate(uint vid, ushort[] parts, byte moveSpeed, byte attackSpeed, byte state, uint[] affects,
        uint guildId, short rankPoints, byte pkMode, uint mountVnum)
    {
        Vid = vid;
        Parts = parts;
        MoveSpeed = moveSpeed;
        AttackSpeed = attackSpeed;
        State = state;
        Affects = affects;
        GuildId = guildId;
        RankPoints = rankPoints;
        PkMode = pkMode;
        MountVnum = mountVnum;
    }
}