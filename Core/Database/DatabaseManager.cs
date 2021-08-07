using System.Data;
using MySqlConnector;
using Serilog;

namespace QuantumCore.Database
{
    public class DatabaseManager
    {
        private static string _accountConnectionString;
        private static string _gameConnectionString;

        public static void Init(string accountString, string gameString)
        {
            Log.Information("Initialize Database Manager");
            _accountConnectionString = accountString;
            _gameConnectionString = gameString;
        }

        public static IDbConnection GetAccountDatabase()
        {
            return new MySqlConnection(_accountConnectionString);
        }

        public static IDbConnection GetGameDatabase()
        {
            return new MySqlConnection(_gameConnectionString);
        }
    }
}