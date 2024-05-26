using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x14, HasSequence = true)]
public readonly ref partial struct ItemDrop
{
    public readonly byte Window;
    public readonly ushort Position;
    public readonly uint Gold;
    public readonly byte Count;

    public ItemDrop(byte window, ushort position, uint gold, byte count)
    {
        Window = window;
        Position = position;
        Gold = gold;
        Count = count;
    }
}