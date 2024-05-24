using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.QuickBar;

[ServerToClientPacket(0x1C)]
public readonly ref partial struct QuickBarAddOut
{
    public readonly byte Position;
    public readonly QuickSlot Slot;

    public QuickBarAddOut(byte position, QuickSlot slot)
    {
        Position = position;
        Slot = slot;
    }
}