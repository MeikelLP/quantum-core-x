using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("gold", "Adds the given amount of gold")]
public class GoldCommand : ICommandHandler<GoldCommandOptions>
{
    private readonly IWorld _world;

    public GoldCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<GoldCommandOptions> context)
    {
        var target = context.Player;
        if (!string.IsNullOrWhiteSpace(context.Arguments.Target))
        {
            target = _world.GetPlayer(context.Arguments.Target);
        }

        if (target is null)
        {
            await context.Player.SendChatMessage("Target not found");
            return;
        }
        
        await target.AddPoint(EPoints.Gold, context.Arguments.Value);
        await target.SendPoints();
    }
}

public class GoldCommandOptions
{
    [Value(0)]
    public int Value { get; set; }
    
    [Value(1)]
    public string Target { get; set; }
}