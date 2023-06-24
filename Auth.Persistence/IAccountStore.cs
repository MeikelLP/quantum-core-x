using QuantumCore.Auth.Persistence.Entities;

namespace QuantumCore.Auth.Persistence;

public interface IAccountStore
{
    Task<Account?> FindByNameAsync(string name);
    Task<Account?> FindByIdAsync(Guid id);
}