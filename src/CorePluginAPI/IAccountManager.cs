namespace QuantumCore.API;

public interface IAccountManager
{
    Task<string?> GetDeleteCodeAsync(Guid accountId);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData?> FindByNameAsync(string userName);
    Task<AccountData> CreateAsync(string userName, string password, string email, string deleteCode);
}
