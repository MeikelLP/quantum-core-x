using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("level", "Sets the level of the current player or of another player")]
public class LevelCommand : ICommandHandler<LevelCommandOptions>
{
    private readonly IWorld _world;

    public LevelCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<LevelCommandOptions> context)
    {
        var target = !string.IsNullOrWhiteSpace(context.Arguments.Target)
            ? _world.GetPlayer(context.Arguments.Target)
            : context.Player;

        if (target is null)
        {
            await context.Player.SendChatMessage("Target not found");
        }
        else
        {
            await target.SetPoint(EPoints.Level, context.Arguments.Level);
            await target.SendPoints();
        }
    }
}

public class LevelCommandOptions
{
    [Value(0, Required = true)]
    public byte Level { get; set; }

    [Value(1)]
    public string Target { get; set; }
}