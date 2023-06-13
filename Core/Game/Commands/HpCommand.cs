using System.Threading.Tasks;
using CommandLine;
using JetBrains.Annotations;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("hp", "Set Hp to other players")]
public class HpCommand : ICommandHandler<HpOtherOptions>
{
    private readonly IWorld _world;

    public HpCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<HpOtherOptions> context)
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
            await target.AddPoint(EPoints.Hp, context.Arguments.Value);
            await target.SendPoints();   
        }
    }
}

public class HpOtherOptions
{
    [Value(0)]
    public int Value { get; set; }

    [Value(1)] 
    [CanBeNull] 
    public string Target { get; set; }
}