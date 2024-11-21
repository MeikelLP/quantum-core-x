using System.Collections.Immutable;

namespace QuantumCore.Game.Commands;

public class CommandValidationException : Exception
{
    public string Command { get; }
    public ImmutableArray<string> Errors { get; set; } = [];

    public CommandValidationException(string command) : base("Command arguments validation failed")
    {
        Command = command;
    }
}
