using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IShop
{
    uint Vid { get; set; }
    string Name { get; set; }
    IReadOnlyList<ShopItem> Items { get; }
    List<IPlayerEntity> Visitors { get; }
    void AddItem(uint itemId, byte count, uint price);
    void Open(IPlayerEntity player);
    Task Buy(IPlayerEntity player, byte position, byte count);
    void Sell(IPlayerEntity player, byte position);
    void Close(IPlayerEntity player, bool sendClose = false);
}
