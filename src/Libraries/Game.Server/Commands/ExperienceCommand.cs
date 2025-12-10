using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Entities;
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

    public Task ExecuteAsync(CommandContext<ExperienceOtherOptions> context)
    {
        var target = context.Player;
        if (!string.IsNullOrWhiteSpace(context.Arguments.Target))
        {
            target = _world.GetPlayer(context.Arguments.Target);
        }

        if (target is null)
        {
            context.Player.SendChatMessage("Target not found!");
        }
        else
        {
            target.AddPoint(EPoint.Experience, context.Arguments.Value);
            target.SendPoints();
        }

        return Task.CompletedTask;
    }
}

public class ExperienceOtherOptions
{
    [Value(0)] public int Value { get; set; }

    [Value(1)] public string? Target { get; set; }
}
