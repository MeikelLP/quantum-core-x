using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x04, HasSequence = true)]
public readonly ref partial struct CreateCharacter
{
    public readonly byte Slot;
    [FixedSizeString(25)] public readonly string Name;
    public readonly ushort Class;
    public readonly byte Appearance;
    [FixedSizeArray(4)] public readonly byte[] Unknown;

    public CreateCharacter(byte slot, string name, ushort @class, byte appearance, byte[] unknown)
    {
        Slot = slot;
        Name = name;
        Class = @class;
        Appearance = appearance;
        Unknown = unknown;
    }
}