using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x08)]
public readonly ref partial struct CreateCharacterSuccess
{
    public readonly byte Slot;
    public readonly Character Character;

    public CreateCharacterSuccess(byte slot, Character character)
    {
        Slot = slot;
        Character = character;
    }
}