using CommandLine;

namespace QuantumCore
{
    [Verb("auth", HelpText = "Starts the authentication server")]
    public class AuthOptions
    {
        [Option("port", HelpText = "Port on which the server should listen")]
        public short Port { get; set; } = 11002;
    }
}