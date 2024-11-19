using CommandLine;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;

namespace QuantumCore.Game.Commands
{
    [Command("kick", "Kick a player from the Server")]
    public class KickCommand : ICommandHandler<KickCommandOptions>
    {
        private readonly IWorld _world;

        public KickCommand(IWorld world)
        {
            _world = world;
        }

        public Task ExecuteAsync(CommandContext<KickCommandOptions> context)
        {
            if (context.Arguments.Target == null)
            {
                context.Player.SendChatMessage("No target given");
                return Task.CompletedTask;
            }

            var target = _world.GetPlayer(context.Arguments.Target);
            if (target is not null)
            {
                _world.DespawnPlayerAsync(target);
                target.Disconnect();
            }
            else
            {
                context.Player.SendChatMessage("Target not found");
            }

            return Task.CompletedTask;
        }
    }

    public class KickCommandOptions
    {
        [Value(0, Required = true)] public string? Target { get; set; }
    }
}