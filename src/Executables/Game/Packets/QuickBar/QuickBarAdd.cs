using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ClientToServerPacket(0x10, HasSequence = true)]
public readonly ref partial struct QuickBarAdd
{
    public readonly byte Position;
    public readonly QuickSlot Slot;

    public QuickBarAdd(byte position, QuickSlot slot)
    {
        Position = position;
        Slot = slot;
    }
}