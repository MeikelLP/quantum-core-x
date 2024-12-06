using QuantumCore.API.Game;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands;

[Command("forgetme", "Clears aggro from all monsters targeting you")]
public class ForgetMeCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        context.Player.ForEachNearbyEntity(e =>
        {
            if (e is MonsterEntity mob && mob.Target == context.Player)
            {
                mob.Target = null;
            }
        });
        return Task.CompletedTask;
    }
}
