using CommandLine;
using QuantumCore.API.Game;

namespace QuantumCore.Game.Commands;

[Command("kill", "Kill target player")]
public class KillCommand : ICommandHandler<KillCommandOptions>
{
    public Task ExecuteAsync(CommandContext<KillCommandOptions> context)
    {
        var target = context.Player.Map?.World.GetPlayer(context.Arguments.Player);
        if (target is null)
        {
            context.Player.SendChatInfo("Target player does not exist.");
        }
        else
        {
            target.Health = 0;
            target.Mana = 0;
            target.Die();
            target.SendPoints();
        }

        return Task.CompletedTask;
    }
}

public class KillCommandOptions
{
    [Value(0, Required = true)] public string Player { get; set; } = "";
}
