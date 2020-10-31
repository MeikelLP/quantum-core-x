using CommandLine;

namespace QuantumCore
{
    [Verb("migrate")]
    public class MigrateOptions : GeneralOptions
    {
        [Option("debug", HelpText = "Enable print of executed sql queries")]
        public bool Debug { get; set; }
    }
}