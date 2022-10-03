using System.Threading.Tasks;
using CommandLine;
using JetBrains.Annotations;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("goto", "Warp to a position")]
    public class GotoCommand : ICommandHandler<GotoCommandOptions>
    {
        private readonly IWorld _world;

        public GotoCommand(IWorld world)
        {
            _world = world;
        }

        public async Task ExecuteAsync(CommandContext<GotoCommandOptions> context)
        {
            if (!string.IsNullOrWhiteSpace(context.Arguments.Map))
            {
                var maps = _world.FindMapsByName(context.Arguments.Map);
                if (maps.Count > 1)
                {
                    await context.Player.SendChatInfo("Map name is ambiguous:");
                    foreach (var map in maps)
                    {
                        await context.Player.SendChatInfo($"- {map.Name}");
                    }

                    return;
                }

                if (maps.Count == 0)
                {
                    await context.Player.SendChatInfo("Unknown map");
                    return;
                }
            
                // todo read goto position from map instead of using center

                var targetMap = maps[0];
                var x = (int)(targetMap.PositionX + targetMap.Width * Map.MapUnit / 2);
                var y = (int)(targetMap.PositionY + targetMap.Height * Map.MapUnit / 2);
                await context.Player.Move(x, y);
            }
            else
            {
                if (context.Arguments.X < 0 || context.Arguments.Y < 0)
                    await context.Player.SendChatInfo("The X and Y position must be positive");
                else
                {
                    var x = (int) context.Player.Map.PositionX + (context.Arguments.X*100);
                    var y = (int) context.Player.Map.PositionY + (context.Arguments.Y*100);
                    await context.Player.Move(x, y);
                }
            }
        }
    }

    public class GotoCommandOptions
    {
        [Option('m', "map")] 
        [CanBeNull] public string Map { get; set; }

        [Value(0)]
        public int X { get; set; }

        [Value(1)]
        public int Y { get; set; }
    }
}
