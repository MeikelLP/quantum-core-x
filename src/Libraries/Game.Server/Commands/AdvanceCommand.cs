using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("advance", "Advances the level of the current player or of another player")]
[Command("a", "Advances the level of the current player or of another player")]
public class AdvanceCommand : ICommandHandler<AdvanceCommandOptions>
{
    private readonly IWorld _world;

    public AdvanceCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<AdvanceCommandOptions> context)
    {
        var target = string.Equals(context.Arguments.Target, "$self", StringComparison.InvariantCultureIgnoreCase)
            ? context.Player
            : _world.GetPlayer(context.Arguments.Target);

        if (target is null)
        {
            context.Player.SendChatInfo("Target not found");
        }
        else
        {
            if (context.Arguments.Level <= 0)
            {
                context.Arguments.Level = 1;
            }

            target.AddPoint(EPoint.Level, context.Arguments.Level);
            target.SendChatInfo($"You have advanced to level {target.GetPoint(EPoint.Level)}");
        }
    }
}

public class AdvanceCommandOptions
{
    [Value(0)] public string Target { get; set; } = "$self";
    [Value(1)] public int Level { get; set; } = 1;
}
