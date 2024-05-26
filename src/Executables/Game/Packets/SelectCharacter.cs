using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x06, HasSequence = true)]
public readonly ref partial struct SelectCharacter
{
    public readonly byte Slot;

    public SelectCharacter(byte slot)
    {
        Slot = slot;
    }
}