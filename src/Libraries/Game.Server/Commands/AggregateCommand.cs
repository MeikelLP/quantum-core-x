using QuantumCore.API.Game;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("aggregate", "Triggers all monsters around you to be aggro and target you")]
public class AggregateCommand : ICommandHandler
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
