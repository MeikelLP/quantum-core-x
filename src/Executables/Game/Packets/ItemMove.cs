using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x0d, HasSequence = true)]
public readonly ref partial struct ItemMove
{
    public readonly byte FromWindow;
    public readonly ushort FromPosition;
    public readonly byte ToWindow;
    public readonly ushort ToPosition;
    public readonly byte Count;

    public ItemMove(byte fromWindow, ushort fromPosition, byte toWindow, ushort toPosition, byte count)
    {
        FromWindow = fromWindow;
        FromPosition = fromPosition;
        ToWindow = toWindow;
        ToPosition = toPosition;
        Count = count;
    }
}