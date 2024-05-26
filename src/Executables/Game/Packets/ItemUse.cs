using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x0b, HasSequence = true)]
public readonly ref partial struct ItemUse
{
    public readonly byte Window;
    public readonly ushort Position;

    public ItemUse(byte window, ushort position)
    {
        Window = window;
        Position = position;
    }
}