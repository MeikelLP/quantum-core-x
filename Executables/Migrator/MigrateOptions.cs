using CommandLine;

namespace QuantumCore.Migrator;

[Verb("migrate")]
public class MigrateOptions
{
    [Option("debug", HelpText = "Enable print of executed sql queries")]
    public bool Debug { get; set; }

    [Option("host", HelpText = "Database host")]
    public string Host { get; set; } = "localhost";

    [Option("port", HelpText = "Database port")]
    public uint Port { get; set; } = 3306;

    [Option("user", HelpText = "Database user")]
    public string User { get; set; } = "root";

    [Option("password", HelpText = "Database user password")]
    public string Password { get; set; } = "";
}