using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x0A)]
public readonly ref partial struct DeleteCharacterSuccess
{
    public readonly byte Slot;

    public DeleteCharacterSuccess(byte slot)
    {
        Slot = slot;
    }
}