using System.Threading.Tasks;
using CommandLine;
using JetBrains.Annotations;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("sp", "Set Sp to other players")]
public class SpCommand : ICommandHandler<SpOtherOptions>
{
    private readonly IWorld _world;

    public SpCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<SpOtherOptions> context)
    {
        var target = context.Player;
        if (!string.IsNullOrWhiteSpace(context.Arguments.Target))
        {
            target = _world.GetPlayer(context.Arguments.Target);
        }

        if (target is null)
        {
            await context.Player.SendChatMessage("Target not found!");
        }
        else
        {
            await target.AddPoint(EPoints.Sp, context.Arguments.Value);
            await target.SendPoints();   
        }
    }
}

public class SpOtherOptions
{
    [Value(0)]
    public int Value { get; set; }

    [Value(1)] 
    [CanBeNull] 
    public string Target { get; set; }
}