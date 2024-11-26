using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("set_maxsp", "Set max sp temporarily")]
public class SetMaxSpCommand : ICommandHandler<SetMaxSpCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetMaxSpCommandOptions> context)
    {
        context.Player.Player.MaxSp = context.Arguments.Value;
        context.Player.Mana = Math.Min(context.Player.Mana, context.Arguments.Value);
        context.Player.SendPoints();

        return Task.CompletedTask;
    }
}

public class SetMaxSpCommandOptions
{
    [Value(0, Required = true)] public uint Value { get; set; }
}
