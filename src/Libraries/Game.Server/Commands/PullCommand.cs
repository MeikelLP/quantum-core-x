using QuantumCore.API.Game;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("pull_monster", "Triggers all monsters around you to be aggro and target you")]
[Command("pull", "Triggers all monsters around you to be aggro and target you")]
public class PullCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.ForEachNearbyEntity(e =>
        {
            if (e is not MonsterEntity) return;
            e.Target = context.Player;
        });
        return Task.CompletedTask;
    }
}
