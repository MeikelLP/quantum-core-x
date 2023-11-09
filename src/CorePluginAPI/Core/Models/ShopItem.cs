namespace QuantumCore.API.Core.Models;

public class ShopItem
{
    public uint ItemId { get; set; }
    public byte Count { get; set; }
    public uint Price { get; set; }
    public byte Position { get; set; }
}