#nullable enable

using Microsoft.EntityFrameworkCore;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public interface IItemRepository
{
    Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(uint playerId, byte window);
    Task<ItemInstance?> GetItemAsync(Guid id);
    Task DeletePlayerItemsAsync(uint playerId);
}

public class ItemRepository : IItemRepository
{
    private readonly GameDbContext _db;

    public ItemRepository(GameDbContext db)
    {
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
}
