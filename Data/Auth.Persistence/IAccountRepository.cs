using QuantumCore.API;

namespace QuantumCore.Auth.Persistence;

public interface IAccountRepository
{
    Task<AccountData?> FindByNameAsync(string name);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task CreateAsync(AccountData account);
}