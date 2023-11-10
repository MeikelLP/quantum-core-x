#nullable enable

using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;

namespace QuantumCore.Game.Persistence;

public class ItemRepository : IItemRepository
{
    private readonly IDbConnection _db;

    public ItemRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Guid>> GetItemIdsForPlayerAsync(Guid player, byte window)
    {
        return await _db.QueryAsync<Guid>(
            "SELECT Id FROM items WHERE PlayerId = @PlayerId AND `Window` = @Window",
            new { PlayerId = player, Window = window });
    }

    public async Task<ItemInstance?> GetItemAsync(Guid id)
    {
        return await _db.GetAsync<ItemInstance>(id);
    }

    public async Task DeletePlayerItemsAsync(Guid playerId)
    {
        await _db.ExecuteAsync("DELETE FROM items WHERE PlayerId=@PlayerId", new { PlayerId = playerId });
    }
}
