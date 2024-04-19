using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game;

public interface IShopsManager
{
    public Task<ShopItem[]> GetShopItems(uint npcVnum);
}

public class ShopsManager : IShopsManager
{
    private readonly IShopsRepository _shopsRepository;
    private readonly IItemManager _itemManager;
    private readonly ILogger<ShopsManager> _logger;

    public ShopsManager(IShopsRepository shopsRepository, IItemManager itemManager, ILogger<ShopsManager> logger)
    {
        _shopsRepository = shopsRepository;
        _itemManager = itemManager;
        _logger = logger;
    }

    public async Task<ShopItem[]> GetShopItems(uint npcVnum)
    {
        // get shops always from db not from cache
        var shopIds = await _shopsRepository.GetShopIdByVnum(npcVnum);
        var shopDataList = new List<ShopItem>();
            foreach (var shopId in shopIds)
            {
                var items = await _shopsRepository.GetShopItems(shopId);
                foreach (var item in items)
                {
                    var itemData = _itemManager.GetItem(item.ItemId);
                    var shopItem = new ShopItem{
                        Count = (byte) item.Count,
                        Price = itemData.SellPrice,
                        ItemId = item.ItemId
                    };
                    shopDataList.Add(shopItem);
                }
            }
        return [.. shopDataList];

    }
}
