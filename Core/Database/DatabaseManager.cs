using System.Data;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace QuantumCore.Database
{
    public class DatabaseManager : IDatabaseManager
    {
        private readonly ILogger<DatabaseManager> _logger;
        private string _accountConnectionString;
        private string _gameConnectionString;

        public DatabaseManager(ILogger<DatabaseManager> logger)
        {
            _logger = logger;
        }
        
        public void Init(string accountString, string gameString)
        {
            _logger.LogInformation("Initialize Database Manager");
            _accountConnectionString = accountString;
            _gameConnectionString = gameString;
        }

        public IDbConnection GetAccountDatabase()
        {
            return new MySqlConnection(_accountConnectionString);
        }

        public IDbConnection GetGameDatabase()
        {
            return new MySqlConnection(_gameConnectionString);
        }
    }
}