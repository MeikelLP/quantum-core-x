using QuantumCore.API.Core.Models;

namespace QuantumCore.Auth.Persistence;

public interface IAccountRepository
{
    Task<AccountData?> FindByNameAsync(string name);
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData> CreateAsync(AccountData account);
}