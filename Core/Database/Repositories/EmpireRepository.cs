using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using QuantumCore.Core.Cache;

namespace QuantumCore.Database;

public interface IEmpireRepository
{
    Task<byte?> GetEmpireForPlayerAsync(Guid accountId);
    Task<byte?> GetTempEmpireForAccountAsync(Guid accountId);
}

public class EmpireRepository : IEmpireRepository
{
    private readonly IDbConnection _db;
    private readonly ICacheManager _cacheManager;

    public EmpireRepository(IDbConnection db, ICacheManager cacheManager)
    {
        _db = db;
        _cacheManager = cacheManager;
    }

    public async Task<byte?> GetEmpireForPlayerAsync(Guid playerId)
    {
        var empireRedisKey = $"empire-pid:{playerId}";
        var cachedEmpire = await _cacheManager.Get<byte?>(empireRedisKey);

        if(cachedEmpire.HasValue)
        {
            // If found in Redis on Player, return the cached empire value
            return cachedEmpire;
        }

        var databaseEmpire = await _db.QueryFirstOrDefaultAsync<byte>(
            "SELECT Empire FROM game.players WHERE Id = @PlayerId", new { PlayerId = playerId });

        if (databaseEmpire > 0)
        {
            await _cacheManager.Set(empireRedisKey, databaseEmpire);
        }

        return databaseEmpire;
    }

    /// <summary>
    /// If no character has been created for the account, the player's selection is kept in the cache. 
    /// Therefore, the value kept in the cache is fetched with this method.
    /// </summary>
    /// <param name="accountId"></param>
    /// <returns></returns>
    public async Task<byte?> GetTempEmpireForAccountAsync(Guid accountId)
    {
        var empireRedisKey = $"empire-aid:{accountId}";
        var cachedEmpire = await _cacheManager.Get<byte?>(empireRedisKey);

        if (cachedEmpire.HasValue)
        {
            // If found in Redis on Account, return the cached empire value
            return cachedEmpire;
        }

        return 0;
    }
}

