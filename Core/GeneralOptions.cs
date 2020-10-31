using CommandLine;

namespace QuantumCore
{
    public class GeneralOptions
    {
        [Option("account-database-host", HelpText = "Database host for account database")]
        public string AccountDatabaseHost { get; set; } = "localhost";
        [Option("account-database-user", HelpText = "Database user for account database")]
        public string AccountDatabaseUser { get; set; } = "root";
        [Option("account-database-password", HelpText = "Database password for account database")]
        public string AccountDatabasePassword { get; set; } = "";
        [Option("account-database", HelpText = "Database for account database")]
        public string AccountDatabase { get; set; } = "account";
        
        [Option("game-database-host", HelpText = "Database host for game database")]
        public string GameDatabaseHost { get; set; } = "localhost";
        [Option("game-database-user", HelpText = "Database user for game database")]
        public string GameDatabaseUser { get; set; } = "root";
        [Option("game-database-password", HelpText = "Database password for game database")]
        public string GameDatabasePassword { get; set; } = "";
        [Option("account-database", HelpText = "Database for account database")]
        public string GameDatabase { get; set; } = "game";

        public string AccountString => $"Server={AccountDatabaseHost};Database={AccountDatabase};Uid={AccountDatabaseUser};Pwd={AccountDatabasePassword}";

        public string GameString => $"Server={GameDatabaseHost};Database={GameDatabase};Uid={GameDatabaseUser};Pwd={GameDatabasePassword}";
    }
}