using System.Data;
using Dapper;
using QuantumCore.API;
using QuantumCore.API.Data;

namespace QuantumCore.Auth.Persistence;

public class AccountRepository : IAccountRepository
{
    private readonly IDbConnection _db;

    public AccountRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<AccountData?> FindByNameAsync(string userName)
    {
        var results = await _db.QueryAsync<AccountData, AccountStatusData, AccountData>(
            "SELECT * FROM account.accounts a JOIN account.account_status s on a.Status = s.Id WHERE a.Username = @Username", (account, status) =>
            {
                account.AccountStatus = status;
                return account;
            }, new { Username = userName });

        return results.FirstOrDefault();
    }

    public async Task<AccountData?> FindByIdAsync(Guid id)
    {
        var results = await _db.QueryAsync<AccountData, AccountStatusData, AccountData>(
            "SELECT * FROM account.accounts a JOIN account.account_status s on a.Status = s.Id WHERE a.Id = @Id", (account, status) =>
            {
                account.AccountStatus = status;
                return account;
            }, new { Id = id });

        return results.FirstOrDefault();
    }

    public async Task CreateAsync(AccountData account)
    {
        var result = await _db.ExecuteAsync("INSERT INTO account.accounts (Id, Username, Password, Email, DeleteCode) " +
                                            "VALUES (@Id, @Username, @Password, @Email, @DeleteCode)", account);

        if (result != 1)
        {
            throw new InvalidOperationException(
                "Creating an account did not result in 1 row changed. This should never happen");
        }
    }

    public async Task<string?> GetDeleteCodeAsync(Guid accountId)
    {
        return await _db.QueryFirstOrDefaultAsync<string>(
            "SELECT DeleteCode FROM account.accounts WHERE Id = @accountId", new
            {
                accountId
            });
    }
}
