using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ClientToServerPacket(0x53, HasSequence = true)]
public readonly ref partial struct ItemGive
{
    public readonly uint TargetVid;
    public readonly byte Window;
    public readonly ushort Position;
    public readonly byte Count;

    public ItemGive(uint targetVid, byte window, ushort position, byte count)
    {
        TargetVid = targetVid;
        Window = window;
        Position = position;
        Count = count;
    }
}