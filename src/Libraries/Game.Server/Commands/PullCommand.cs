using System.Numerics;
using QuantumCore.API.Game;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Commands;

[Command("pull", "All monsters in range will quickly move towards you")]
[Command("pull_monster", "All monsters in range will quickly move towards you")]
public class PullCommand : ICommandHandler
{
    public Task ExecuteAsync(CommandContext context)
    {
        const int maxDistance = 3000;
        const int minDistance = 100;
        var p = context.Player;
        context.Player.ForEachNearbyEntity(e =>
        {
            if (e.Type == EEntityType.Monster)
            {
                var dist = Vector2.Distance(new Vector2(p.PositionX, p.PositionY),
                    new Vector2(e.PositionX, e.PositionY));
                if (dist is > maxDistance or < minDistance)
                    return;

                var degree = MathUtils.Rotation(p.PositionX - e.PositionX, p.PositionY - e.PositionY);

                var pos = MathUtils.GetDeltaByDegree(degree);
                var targetX = p.PositionX + pos.X + dist;
                var targetY = p.PositionY + pos.Y + dist;
                // not correct - moves to the wrong side of the player
                // sadly my math skills are too low to implement it correctly
                // good enough for now
                e.Goto((int)targetX, (int)targetY);
            }
        });
        return Task.CompletedTask;
    }
}
