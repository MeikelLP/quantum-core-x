using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence;

public interface IAccountManager
{
    Task<Account?> FindByNameAsync(string name);
}