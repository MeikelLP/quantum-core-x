using QuantumCore.API.Game.Types.Items;
using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets;

[Packet(0x15, EDirection.OUTGOING)]
[PacketGenerator]
public partial class SetItem
{
    [Field(0)] public WindowType Window { get; set; }
    [Field(1)] public ushort Position { get; set; }
    [Field(2)] public uint ItemId { get; set; }
    [Field(3)] public byte Count { get; set; }
    [Field(4)] public uint Flags { get; set; }
    [Field(5)] public uint AntiFlags { get; set; }
    [Field(6)] public uint Highlight { get; set; }
    [Field(7, ArrayLength = 3)] public uint[] Sockets { get; set; } = new uint[3];

    [Field(8, ArrayLength = 7)]
    public ItemBonus[] Bonuses { get; set; } = new ItemBonus[7]
    {
        new ItemBonus(), new ItemBonus(), new ItemBonus(), new ItemBonus(),
        new ItemBonus(), new ItemBonus(), new ItemBonus()
    };
}
