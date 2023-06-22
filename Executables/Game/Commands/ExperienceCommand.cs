using CommandLine;
using JetBrains.Annotations;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("exp", "Give experience to other players")]
public class ExperienceCommand : ICommandHandler<ExperienceOtherOptions>
{
    private readonly IWorld _world;

    public ExperienceCommand(IWorld world)
    {
        _world = world;
    }

    public async Task ExecuteAsync(CommandContext<ExperienceOtherOptions> context)
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
            await target.AddPoint(EPoints.Experience, context.Arguments.Value);
            await target.SendPoints();   
        }
    }
}

public class ExperienceOtherOptions
{
    [Value(0)]
    public int Value { get; set; }

    [Value(1)] 
    [CanBeNull] 
    public string Target { get; set; }
}