using QuantumCore.API.Data;

namespace QuantumCore;

public class AccountManager : IAccountManager
{
    private readonly IAccountRepository _repository;

    public AccountManager(IAccountRepository repository)
    {
        _repository = repository;
    }

    public Task<string> GetDeleteCodeAsync(Guid accountId)
    {
        return _repository.GetDeleteCodeAsync(accountId);
    }
}
