#nullable enable
using System;
using System.Threading.Tasks;

namespace QuantumCore.API;

public interface IAccountManager
{
    Task<AccountData?> FindByIdAsync(Guid id);
    Task<AccountData?> FindByNameAsync(string userName);
    Task<AccountData> CreateAsync(string userName, string password, string email, string deleteCode);
}