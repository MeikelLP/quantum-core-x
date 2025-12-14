using CommandLine;
using Microsoft.Extensions.Logging;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Commands;

[Command("goto", "Warp to a position")]
public class GotoCommand : ICommandHandler<GotoCommandOptions>
{
    private readonly IWorld _world;
    private readonly ILogger<GotoCommand> _logger;

    public GotoCommand(IWorld world, ILogger<GotoCommand> logger)
    {
        _world = world;
        _logger = logger;
    }

    public Task ExecuteAsync(CommandContext<GotoCommandOptions> context)
    {
        if (!string.IsNullOrWhiteSpace(context.Arguments.Map))
        {
            var maps = _world.FindMapsByName(context.Arguments.Map);
            if (maps.Count > 1)
            {
                context.Player.SendChatInfo("Map name is ambiguous:");
                foreach (var map in maps)
                {
                    context.Player.SendChatInfo($"- {map.Name}");
                }

                return Task.CompletedTask;
            }

            if (maps.Count == 0)
            {
                context.Player.SendChatInfo("Unknown map");
                return Task.CompletedTask;
            }

            // todo read goto position from map instead of using center

            var targetMap = maps[0];
            var x = (int)(targetMap.Position.X + targetMap.Width * Map.MAP_UNIT / 2);
            var y = (int)(targetMap.Position.Y + targetMap.Height * Map.MAP_UNIT / 2);
            context.Player.Move(x, y);
        }
        else
        {
            if (context.Player.Map is null)
            {
                _logger.LogCritical("Player's map is null, this should never happen");
                context.Player.Connection.Close();
                return Task.CompletedTask;
            }

            if (context.Arguments.X < 0 || context.Arguments.Y < 0)
                context.Player.SendChatInfo("The X and Y position must be positive");
            else
            {
                var x = (int)context.Player.Map.Position.X + (context.Arguments.X * 100);
                var y = (int)context.Player.Map.Position.Y + (context.Arguments.Y * 100);
                context.Player.Move(x, y);
            }
        }

        context.Player.ShowEntity(context.Player.Connection);
        return Task.CompletedTask;
    }
}

public class GotoCommandOptions
{
    [Option('m', "map")] public string? Map { get; set; }

    [Value(0)] public int X { get; set; }

    [Value(1)] public int Y { get; set; }
}
