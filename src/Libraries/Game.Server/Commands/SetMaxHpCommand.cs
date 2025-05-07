using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("set_maxhp", "Set max hp temporarily")]
public class SetMaxHpCommand : ICommandHandler<SetMaxHpCommandOptions>
{
    public Task ExecuteAsync(CommandContext<SetMaxHpCommandOptions> context)
    {
        context.Player.Player.MaxHp = context.Arguments.Value;
        context.Player.Health = Math.Min(context.Player.Health, context.Arguments.Value);
        context.Player.SendPoints();

        return Task.CompletedTask;
    }
}

public class SetMaxHpCommandOptions
{
    [Value(0, Required = true)] public uint Value { get; set; }
}
