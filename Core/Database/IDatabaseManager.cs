using System.Data;

namespace QuantumCore.Database;

public interface IDatabaseManager
{
    void Init(string accountString, string gameString);
    IDbConnection GetAccountDatabase();
    IDbConnection GetGameDatabase();
}