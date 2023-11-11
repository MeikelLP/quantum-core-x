using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using QuantumCore.Core.Cache;

namespace QuantumCore.Database;

public interface IEmpireRepository
{
    Task<byte?> GetEmpireForAccountAsync(Guid accountId);
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

    public async Task<byte?> GetEmpireForAccountAsync(Guid accountId)
    {
        var empireRedisKey = $"empire:{accountId}";
        var cachedEmpire = await _cacheManager.Get<byte?>(empireRedisKey);

        if(cachedEmpire.HasValue)
        {
            // If found in Redis, return the cached empire value
            return cachedEmpire;
        }

        var databaseEmpire = await _db.QueryFirstOrDefaultAsync<byte>(
            "SELECT Empire FROM game.players WHERE AccountId = @AccountId", new { AccountId = accountId });

        if (databaseEmpire > 0)
        {
            await _cacheManager.Set(empireRedisKey, databaseEmpire);
        }

        return databaseEmpire;
    }
}

