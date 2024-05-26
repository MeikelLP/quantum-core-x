using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[ServerToClientPacket(0x15)]
public readonly ref partial struct SetItem
{
    public readonly byte Window;
    public readonly ushort Position;
    public readonly uint ItemId;
    public readonly byte Count;
    public readonly uint Flags;
    public readonly uint AnitFlags;
    public readonly uint Highlight;
    [FixedSizeArray(3)] public readonly uint[] Sockets;
    [FixedSizeArray(7)] public readonly ItemBonus[] Bonuses;

    public SetItem(byte window, ushort position, uint itemId, byte count, uint flags, uint anitFlags, uint highlight,
        uint[] sockets, ItemBonus[] bonuses)
    {
        Window = window;
        Position = position;
        ItemId = itemId;
        Count = count;
        Flags = flags;
        AnitFlags = anitFlags;
        Highlight = highlight;
        Sockets = sockets;
        Bonuses = bonuses;
    }
}