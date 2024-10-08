using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands;

[Command("advance", "Advances the level of the current player or of another player", "a")]
public class AdvanceCommand : ICommandHandler<AdvanceCommandOptions>
{
    private readonly IWorld _world;
    private readonly ILogger<AdvanceCommand> _logger;

    public AdvanceCommand(IWorld world, ILogger<AdvanceCommand> logger)
    {
        _world = world;
        _logger = logger;
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

            target.AddPoint(EPoints.Level, context.Arguments.Level);
            _logger.LogInformation("Advancing {Target} to level {Level} ({Point})", target.Name, context.Arguments.Level, target.GetPoint(EPoints.Level));
            target.SendChatInfo($"You have advanced to level {target.GetPoint(EPoints.Level)}");
        }
    }
}

public class AdvanceCommandOptions
{
    [Value(0)] public string Target { get; set; } = "$self";
    [Value(1, Required = true)] public int Level { get; set; }
}
