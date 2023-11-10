namespace QuantumCore.API.Data;

public interface IEmpireRepository
{
    Task<byte?> GetEmpireForAccountAsync(Guid accountId);
}
