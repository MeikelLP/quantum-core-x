#nullable enable

using Microsoft.EntityFrameworkCore;
using QuantumCore.API.Core.Models;
using QuantumCore.Caching;
using QuantumCore.Game.Persistence.Entities;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public interface IItemRepository
{
    Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(uint playerId, byte window);
    Task<ItemInstance?> GetItemAsync(Guid id);
    Task DeletePlayerItemsAsync(uint playerId);
    Task DeletePlayerItemAsync(uint playerId, uint itemId);
    Task SaveItemAsync(ItemInstance item);
}

public class ItemRepository : IItemRepository
{
    private readonly IRedisStore _cacheManager;
    private readonly GameDbContext _db;

    public ItemRepository(ICacheManager cacheManager, GameDbContext db)
    {
        _cacheManager = cacheManager.Server;
        _db = db;
    }

    public async Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(uint playerId, byte window)
    {
        return await _db.Items
            .Where(x => x.PlayerId == playerId && x.Window == window)
            .Select(x => x.Id)
            .ToArrayAsync();
    }

    public async Task<ItemInstance?> GetItemAsync(Guid id)
    {
        return await _db.Items
            .Where(x => x.Id == id)
            .SelectInstance()
            .FirstOrDefaultAsync();
    }

    public async Task DeletePlayerItemsAsync(uint playerId)
    {
        await _db.Items.Where(x => x.PlayerId == playerId).ExecuteDeleteAsync();
    }

    public async Task DeletePlayerItemAsync(uint playerId, uint itemId)
    {
        await _db.Items.Where(x => x.PlayerId == playerId && x.ItemId == itemId).ExecuteDeleteAsync();
    }

    public async Task SaveItemAsync(ItemInstance item)
    {
        _db.Items.Add(new Item
        {
            Id = Guid.NewGuid(),
            PlayerId = item.PlayerId,
            ItemId = item.ItemId,
            Window = item.Window,
            Position = item.Position,
            Count = item.Count,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });
        await _db.SaveChangesAsync();

        string key = $"item:{item.Id}";
        await _cacheManager.Set(key, item);
    }
}
