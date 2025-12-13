using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Entities;

namespace QuantumCore.Game.Commands;

[Command("stat", "Adds a status point")]
[CommandNoPermission]
public class StatCommand : ICommandHandler<StatCommandOptions>
{
    public Task ExecuteAsync(CommandContext<StatCommandOptions> context)
    {
        if (context.Player.GetPoint(EPoint.StatusPoints) <= 0)
        {
            return Task.CompletedTask;
        }

        context.Player.AddPoint(context.Arguments.Point, 1);
        context.Player.AddPoint(EPoint.StatusPoints, -1);
        context.Player.SendPoints();
        return Task.CompletedTask;
    }
}

public class StatCommandOptions
{
    [Value(0)] public EPoint Point { get; set; }
}
