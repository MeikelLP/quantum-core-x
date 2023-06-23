using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;

namespace QuantumCore.Game.Commands;

[Command("stat", "Adds a status point")]
[CommandNoPermission]
public class StatCommand : ICommandHandler<StatCommandOptions>
{
    public async Task ExecuteAsync(CommandContext<StatCommandOptions> context)
    {
        if (context.Player.GetPoint(EPoints.StatusPoints) <= 0)
        {
            return;
        }
        
        await context.Player.AddPoint(context.Arguments.Point, 1);
        await context.Player.AddPoint(EPoints.StatusPoints, -1);
        await context.Player.SendPoints();
    }
}

public class StatCommandOptions
{
    [Value(0)]
    public EPoints Point { get; set; }
}