using System.Data;
using Dapper;
using QuantumCore.API.Core.Models;

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
       var players = await _db.QueryAsync<PlayerData>("SELECT * FROM game.players WHERE AccountId = @AccountId ORDER BY CreatedAt",
            new { AccountId = accountId });

       return players.ToArray();
    }

    public async Task<bool> IsNameInUseAsync(string name)
    {
        var count = await _db.QuerySingleAsync<int>("SELECT COUNT(*) FROM game.players WHERE Name = @Name", new {Name = name});
        return count > 0;
    }

    public async Task CreateAsync(PlayerData player)
    {
        var result = await _db.ExecuteAsync(@"
INSERT INTO game.players (Id, AccountId, PlayerClass, SkillGroup, PlayTime, Level, Experience, Gold, St, Ht, Dx, Iq, PositionX, PositionY, Health, Mana, Stamina, BodyPart, HairPart, Name, GivenStatusPoints, AvailableStatusPoints)
VALUES (@Id, @AccountId, @PlayerClass, @SkillGroup, @PlayTime, @Level, @Experience, @Gold, @St, @Ht, @Dx, @Iq, @PositionX, @PositionY, @Health, @Mana, @Stamina, @BodyPart, @HairPart, @Name, @GivenStatusPoints, @AvailableStatusPoints)", player);
        if (result != 1)
        {
            throw new Exception("Failed to create player");
        }
    }

    public async Task DeletePlayerAsync(PlayerData player)
    {
        await _db.ExecuteAsync("""
            START TRANSACTION;
            INSERT INTO game.deleted_players (Id, AccountId, PlayerClass, SkillGroup, PlayTime, Level, Experience, Gold, St, Ht, Dx, Iq, PositionX, PositionY, Health, Mana, Stamina, BodyPart, HairPart, Name, DeletedAt)
            VALUES (@Id, @AccountId, @PlayerClass, @SkillGroup, @PlayTime, @Level, @Experience, @Gold, @St, @Ht, @Dx, @Iq, @PositionX, @PositionY, @Health, @Mana, @Stamina, @BodyPart, @HairPart, @Name, UTC_TIMESTAMP());
            DELETE FROM game.items WHERE PlayerId = @Id;
            DELETE FROM game.players WHERE Id = @Id;
            COMMIT;
            """, player);
    }

    public async Task<PlayerData?> GetPlayerAsync(Guid playerId)
    {
        return await _db.QueryFirstOrDefaultAsync<PlayerData>("SELECT * FROM game.players WHERE Id = @PlayerId", 
            new {PlayerId = playerId});
    }
}