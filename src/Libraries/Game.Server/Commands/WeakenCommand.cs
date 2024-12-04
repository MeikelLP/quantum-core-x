using QuantumCore.API.Game;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("weak", "Sets health of nearby monsters to 1")]
[Command("weaken", "Sets health of nearby monsters to 1")]
public class WeakenCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.ForEachNearbyEntity(e =>
        {
            if (e is MonsterEntity)
            {
                e.Health = 1;
            }
        });

        return Task.CompletedTask;
    }
}
