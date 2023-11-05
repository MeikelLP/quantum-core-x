using System.Data;
using Dapper;
using QuantumCore.API.Data;

namespace QuantumCore.Database.Repositories;

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
