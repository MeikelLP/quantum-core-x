using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("mspd", "Change your character move speed")]
public class MspdCommand : ICommandHandler<MspdCommandOptions>
{
    public Task ExecuteAsync(CommandContext<MspdCommandOptions> context)
    {
        context.Player.MovementSpeed = context.Arguments.Value;
        context.Player.SendPoints();
        context.Player.SendCharacterUpdate();
        return Task.CompletedTask;
    }
}

public class MspdCommandOptions
{
    [Value(0, Required = true)] public byte Value { get; set; }
}
