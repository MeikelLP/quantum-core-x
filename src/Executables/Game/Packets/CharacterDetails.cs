using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x71)]
public readonly ref partial struct CharacterDetails
{
    public readonly uint Vid;
    public readonly ushort Class;
    [FixedSizeString(25)] public readonly string Name;
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly int PositionZ;
    public readonly byte Empire;
    public readonly byte SkillGroup;

    public CharacterDetails(uint vid, ushort @class, string name, int positionX, int positionY, int positionZ,
        byte empire, byte skillGroup)
    {
        Vid = vid;
        Class = @class;
        Name = name;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        Empire = empire;
        SkillGroup = skillGroup;
    }
}