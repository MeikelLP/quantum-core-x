using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;

namespace QuantumCore.API;

public interface IShop
{
    uint Vid { get; set; }
    string Name { get; set; }
    IReadOnlyList<ShopItem> Items { get; }
#pragma warning disable CA1002
    List<IPlayerEntity> Visitors { get; }
#pragma warning restore CA1002
    void AddItem(uint itemId, byte count, uint price);
    void Open(IPlayerEntity player);
    Task BuyAsync(IPlayerEntity player, byte position, byte count);
    Task SellAsync(IPlayerEntity player, byte position);
    void Close(IPlayerEntity player, bool sendClose = false);
}