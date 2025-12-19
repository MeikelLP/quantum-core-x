using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Extensions;

public static class GameExtensions
{
    public static IEnumerable<IPlayerEntity> GetNearbyPlayers(this IEntity entity)
    {
        foreach (var nearbyEntity in entity.NearbyEntities)
        {
            if (nearbyEntity is IPlayerEntity p)
            {
                yield return p;
            }
        }
    }

    public static double DistanceTo(this IEntity e1, IEntity e2)
    {
        return MathUtils.Distance(e1.PositionX, e1.PositionY, e2.PositionX, e2.PositionY);
    }
}
