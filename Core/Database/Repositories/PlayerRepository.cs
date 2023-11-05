using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Data;

namespace QuantumCore.Database.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnection _db;

    public PlayerRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task DeleteAsync(PlayerData player)
    {
        var delPlayer = new PlayerDeleted(player);
        await _db.InsertAsync(delPlayer); // add the player to the players_deleted table
        await _db.DeleteAsync(player); // delete the player from the players table
    }

    public Task<PlayerData> GetPlayerAsync(Guid playerId)
    {
        return _db.GetAsync<PlayerData>(playerId);
    }

    public async Task<IEnumerable<Guid>> GetPlayerIdsForAccountAsync(Guid account)
    {
        return await _db.QueryAsync<Guid>("SELECT Id FROM players WHERE AccountId = @AccountId",
            new { AccountId = account });
    }
}
