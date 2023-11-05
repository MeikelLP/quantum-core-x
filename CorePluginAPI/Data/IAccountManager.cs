namespace QuantumCore.API.Data;

public interface IAccountManager
{
    Task<string> GetDeleteCodeAsync(Guid accountId);
}
