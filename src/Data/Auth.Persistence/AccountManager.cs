using QuantumCore.API;
using QuantumCore.API.Core.Models;

namespace QuantumCore.Auth.Persistence;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _repository;
    private readonly IPasswordHasher _passwordHasher;

    public AccountManager(IAccountRepository repository, IPasswordHasher passwordHasher)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
    }

    public Task<AccountData?> FindByIdAsync(Guid id)
    {
        return _repository.FindByIdAsync(id);
    }

    public Task<AccountData?> FindByNameAsync(string userName)
    {
        return _repository.FindByNameAsync(userName);
    }

    public async Task<AccountData> CreateAsync(string userName, string password, string email, string deleteCode)
    {
        var existing = await FindByNameAsync(userName);

        if (existing is not null)
        {
            throw new InvalidOperationException($"Account with \"{userName}\" already exists");
        }

        var account = new AccountData
        {
            Id = Guid.NewGuid(),
            Username = userName,
            Email = email,
            DeleteCode = deleteCode,
            Status = 1,
            Password = "",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var hashedPassword = _passwordHasher.HashPassword(account, password);
        account.Password = hashedPassword;

        return await _repository.CreateAsync(account);
    }
}
