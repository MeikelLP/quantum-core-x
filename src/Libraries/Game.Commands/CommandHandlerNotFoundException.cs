namespace QuantumCore.Game.Commands;

public class CommandHandlerNotFoundException : Exception
{
    public string Command { get; }

    public CommandHandlerNotFoundException(string command) : base(
        $"Command handler for command \"{command}\" could not be found")
    {
        Command = command;
    }
}