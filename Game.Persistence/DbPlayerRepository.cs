using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using QuantumCore.API.Core.Models;
using QuantumCore.Game.Persistence.Extensions;

namespace QuantumCore.Game.Persistence;

public class DbPlayerRepository : IDbPlayerRepository
{
    private readonly IDbConnection _db;

    public DbPlayerRepository(IDbConnection db)
    {
        _db = db;
    }
    
    public async Task<PlayerData[]> GetPlayersAsync(Guid accountId)
    {
       var players = await _db.QueryAsync<PlayerData>("SELECT * FROM players WHERE AccountId = @AccountId",
            new { AccountId = accountId });

       return players.ToArray();
    }

    public async Task<bool> IsNameInUseAsync(string name)
    {
        var count = await _db.QuerySingleAsync<int>("SELECT COUNT(*) FROM players WHERE Name = @Name", new {Name = name});
        return count > 0;
    }

    public async Task CreateAsync(PlayerData player)
    {
        var result = await _db.ExecuteAsync(@"
INSERT INTO players (Id, AccountId, PlayerClass, SkillGroup, PlayTime, Level, Experience, Gold, St, Ht, Dx, Iq, PositionX, PositionY, Health, Mana, Stamina, BodyPart, HairPart, CreatedAt, UpdatedAt, Name, GivenStatusPoints, AvailableStatusPoints)
VALUES (@Id, @AccountId, @PlayerClass, @SkillGroup, @PlayTime, @Level, @Experience, @Gold, @St, @Ht, @Dx, @Iq, @PositionX, @PositionY, @Health, @Mana, @Stamina, @BodyPart, @HairPart, @CreatedAt, @UpdatedAt, @Name, @GivenStatusPoints, @AvailableStatusPoints)", player);
        if (result != 1)
        {
            throw new Exception("Failed to create player");
        }
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        var delPlayer = player.ToPlayerDeleted();
        await _db.InsertAsync(delPlayer); // add the player to the players_deleted table

        await _db.ExecuteAsync("DELETE FROM players WHERE Id = @Id;" +
                                  "DELETE FROM items WHERE PlayerId = @Id", new { Id = player.Id });
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid playerId)
    {
        return await _db.QueryFirstAsync<PlayerData>("SELECT * FROM players WHERE Id = @PlayerId", 
            new {PlayerId = playerId});
    }
}