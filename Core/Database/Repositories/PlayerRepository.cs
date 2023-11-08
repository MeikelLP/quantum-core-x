using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;

namespace QuantumCore.Database.Repositories;

public interface IPlayerRepository
{
    Task DeleteAsync(Player player);
    Task<Player> GetPlayerAsync(Guid playerId);
    Task<IEnumerable<Guid>> GetPlayerIdsForAccountAsync(Guid account);
}

public class PlayerRepository : IPlayerRepository
{
    private readonly IDbConnection _db;

    public PlayerRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task DeleteAsync(Player player)
    {
        var delPlayer = new PlayerDeleted(player);
        await _db.InsertAsync(delPlayer); // add the player to the players_deleted table
        await _db.DeleteAsync(player); // delete the player from the players table
    }

    public Task<Player> GetPlayerAsync(Guid playerId)
    {
        return _db.GetAsync<Player>(playerId);
    }

    public async Task<IEnumerable<Guid>> GetPlayerIdsForAccountAsync(Guid account)
    {
        var results = await _db.QueryAsync<string>("SELECT Id FROM players WHERE AccountId = @AccountId",
            new { AccountId = account });
        return results.Select(Guid.Parse);
    }
}

public interface IAccountRepository
{
    Task<string> GetDeleteCodeAsync(Guid accountId);
}

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnection _db;

    public AccountRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<string> GetDeleteCodeAsync(Guid accountId)
    {
        return await _db.QueryFirstOrDefaultAsync<string>("SELECT DeleteCode FROM accounts WHERE Id = @Id",
            new { Id = accountId });
    }
}
