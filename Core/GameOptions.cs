using CommandLine;

namespace QuantumCore
{
    [Verb("game", HelpText = "Starts the game server")]
    public class GameOptions : GeneralOptions
    {
        [Option("port", HelpText = "Port on which the server should listen")]
        public short Port { get; set; }
    }
}