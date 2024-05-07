#nullable enable

using System.Data;
using Dapper;
using QuantumCore.Game.Persistence.Entities;

namespace QuantumCore.Game.Persistence;

public interface IShopsRepository
{
    public Task<IEnumerable<Guid>> GetShopIdByVnum(uint npcVnum);
    public Task<IEnumerable<ShopItems>> GetShopItems(Guid shop);
}

public class ShopsRepository : IShopsRepository
{
    private readonly IDbConnection _db;

    public ShopsRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Guid>> GetShopIdByVnum(uint npcVnum)
    {
        return await _db.QueryAsync<Guid>(
            "SELECT id FROM shops WHERE Vnum = @NpcVnum",
            new { NpcVnum = npcVnum });
    }

    public async Task<IEnumerable<ShopItems>> GetShopItems(Guid shop)
    {
        return await _db.QueryAsync<ShopItems>(
            "SELECT * FROM shop_items WHERE ShopId = @ShopId",
            new { ShopId = shop });
    }

}
