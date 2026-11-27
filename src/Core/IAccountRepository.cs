using QuantumCore.Models;

namespace QuantumCore;

public interface IAccountRepository
{
    Task<AccountData?> FindByNameAsync(string name);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData> CreateAsync(AccountData account);
}
