using QuantumCore.API.Core.Models;

namespace QuantumCore.API;

public interface IAccountRepository
{
    Task<AccountData?> FindByNameAsync(string name);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData> CreateAsync(AccountData account);
}
