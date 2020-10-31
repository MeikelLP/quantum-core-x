using System.Data;
using MySql.Data.MySqlClient;

namespace QuantumCore.Database
{
    public class DatabaseManager
    {
        private static string _accountConnectionString;
        private static string _gameConnectionString;

        public static void Init(string accountString, string gameString)
        {
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