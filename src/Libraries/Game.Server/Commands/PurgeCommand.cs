using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Commands;

[Command("purge", "Remove all mobs and around yourself")]
public class PurgeCommand : ICommandHandler<PurgeCommandOptions>
{
    public Task ExecuteAsync(CommandContext<PurgeCommandOptions> context)
    {
        const int maxDistance = 10000;
        var p = context.Player;
        var all = context.Arguments.Argument == PurgeCommandOption.All;
        foreach (var e in context.Player.Map!.Entities)
        {
            if (e is IPlayerEntity) continue;
            var distance = MathUtils.Distance(e.PositionX, e.PositionY, p.PositionX, p.PositionY);

            if (!all && distance >= maxDistance) continue;

            context.Player.Map.DespawnEntity(e);
        }

        return Task.CompletedTask;
    }
}

public class PurgeCommandOptions
{
    [Value(0, HelpText = "If \"all\" => Will remove all on the current map")]
    public PurgeCommandOption? Argument { get; set; }
}

public enum PurgeCommandOption
{
    All = 1
}
