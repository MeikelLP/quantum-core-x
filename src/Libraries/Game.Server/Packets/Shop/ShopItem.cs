using QuantumCore.Game.Packets.General;
using QuantumCore.Networking;

namespace QuantumCore.Game.Packets.Shop
{
    public class ShopItem
    {
        [Field(0)] public uint ItemId { get; set; }

        [Field(1)] public uint Price { get; set; }

        [Field(2)] public byte Count { get; set; }

        [Field(3)] public byte Position { get; set; }

        [Field(4, ArrayLength = 3)] public uint[] Sockets { get; set; } = new uint[3];

        [Field(5, ArrayLength = 7)] public ItemBonus[] Bonuses { get; set; } = new ItemBonus[7];
    }
}