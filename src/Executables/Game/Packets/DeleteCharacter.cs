using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x05, HasSequence = true)]
public readonly ref partial struct DeleteCharacter
{
    public readonly byte Slot;
    [FixedSizeString(8)] public readonly string Code;

    public DeleteCharacter(byte slot, string code)
    {
        Slot = slot;
        Code = code;
    }
}