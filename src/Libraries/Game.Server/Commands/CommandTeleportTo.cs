using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("tp", "Teleport to another player")]
    public class CommandTeleportTo : ICommandHandler<TeleportToOptions>
    {
        private readonly IWorld _world;

        public CommandTeleportTo(IWorld world)
        {
            _world = world;
        }

        public Task ExecuteAsync(CommandContext<TeleportToOptions> ctx)
        {
            var dest = _world.GetPlayer(ctx.Arguments.Destination);

            if (dest is null)
            {
                ctx.Player.SendChatInfo("Destination not found");
            }
            else
            {
                ctx.Player.SendChatInfo($"Teleporting to player {dest.Name}");
                ctx.Player.Move(dest.PositionX, dest.PositionY);
            }

            return Task.CompletedTask;
        }
    }

    public class TeleportToOptions
    {
        [Value(1)] public string Destination { get; set; } = "";
    }
}