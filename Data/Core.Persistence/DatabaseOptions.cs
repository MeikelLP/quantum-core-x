using System.ComponentModel.DataAnnotations;
using MySqlConnector;

namespace QuantumCore;

public class DatabaseOptions
{
    public string Host { get; set; } = "localhost";

    public uint Port { get; set; } = 3306;

    public string User { get; set; } = "root";

    [Required]
    public string Password { get; set; } = "";

    [Required] public string Database { get; set; } = "";

    public string ConnectionString => new MySqlConnectionStringBuilder {
        Port = Port,
        Database = Database,
        Server = Host,
        Password = Password,
        UserID = User
    }.ToString();
}