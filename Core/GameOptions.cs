using CommandLine;

namespace QuantumCore
{
    [Verb("game", HelpText = "Starts the game server")]
    public class GameOptions : GeneralOptions
    {
        [Option("port", HelpText = "Port on which the server should listen")]
        public short Port { get; set; } = 13001;
        
        [Option("ip-address", HelpText = "Set an explicit public ip address of this core")]
        public string IpAddress { get; set; }
    }
}
