using QuantumCore.API.Game;
using QuantumCore.API.Game.Types;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("attract_ranger", "Triggers all ranged monsters around you to be aggro and target you")]
public class AttractRangerCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.ForEachNearbyEntity(e =>
        {
            if (e is not MonsterEntity monster || monster.GetBattleType() != EBattleType.Range) return;
            e.Target = context.Player;
        });
        return Task.CompletedTask;
    }
}
