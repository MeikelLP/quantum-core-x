using QuantumCore.API.Core.Models;

namespace QuantumCore.Auth.Persistence;

public interface IAccountManager
{
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData?> FindByNameAsync(string userName);
    Task<AccountData> CreateAsync(string userName, string password, string email, string deleteCode);
}
