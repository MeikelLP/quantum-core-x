using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

public readonly struct Character
{
    public readonly uint Id;
    [FixedSizeString(25)] public readonly string Name;
    public readonly byte Class;
    public readonly byte Level;
    public readonly uint Playtime;
    public readonly byte St;
    public readonly byte Ht;
    public readonly byte Dx;
    public readonly byte Iq;
    public readonly ushort BodyPart;
    public readonly byte NameChange;
    public readonly ushort HairPort;
    public readonly uint Unknown;
    public readonly int PositionX;
    public readonly int PositionY;
    public readonly int Ip;
    public readonly ushort Port;
    public readonly byte SkillGroup;

    public Character(uint id, string name, byte @class, byte level, uint playtime, byte st, byte ht, byte dx, byte iq,
        ushort bodyPart, byte nameChange, ushort hairPort, uint unknown, int positionX, int positionY, int ip,
        ushort port, byte skillGroup)
    {
        Id = id;
        Name = name;
        Class = @class;
        Level = level;
        Playtime = playtime;
        St = st;
        Ht = ht;
        Dx = dx;
        Iq = iq;
        BodyPart = bodyPart;
        NameChange = nameChange;
        HairPort = hairPort;
        Unknown = unknown;
        PositionX = positionX;
        PositionY = positionY;
        Ip = ip;
        Port = port;
        SkillGroup = skillGroup;
    }
}