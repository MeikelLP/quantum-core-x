using System.Data;
using Dapper;
using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence;

public class AccountManager : IAccountManager
{
    private readonly IDbConnection _db;

    public AccountManager(IDbConnection db)
    {
        _db = db;
    }
    
    public async Task<Account?> FindByNameAsync(string userName)
    {
        var results = await _db.QueryAsync<Account, AccountStatus, Account>(
            "SELECT * FROM accounts a JOIN account_status s on a.Status = s.Id WHERE a.Username = @Username", (account, status) =>
            {
                account.AccountStatus = status;
                return account;
            }, new { Username = userName });

        return results.First();
    }
}