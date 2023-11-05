namespace QuantumCore.API.Data;

public interface IAccountRepository
{
    Task<string> GetDeleteCodeAsync(Guid accountId);
}
