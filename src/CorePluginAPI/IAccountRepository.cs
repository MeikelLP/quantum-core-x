namespace QuantumCore.API.Data;

public interface IAccountRepository
{
    Task<AccountData?> FindByNameAsync(string name);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task CreateAsync(AccountData account);
    Task<string?> GetDeleteCodeAsync(Guid accountId);
}
