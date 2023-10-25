using System.Data;
using Dapper;

namespace QuantumCore.Game.Persistence;

public interface IEmpireRepository
{
    Task<byte?> GetEmpireForAccountAsync(Guid accountId);
}

public class EmpireRepository : IEmpireRepository
{
    private readonly IDbConnection _db;

    public EmpireRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<byte?> GetEmpireForAccountAsync(Guid accountId)
    {
        return await _db.QueryFirstOrDefaultAsync<byte>(
            "SELECT Empire FROM account.accounts WHERE Id = @AccountId", new { AccountId = accountId });
    }
}

