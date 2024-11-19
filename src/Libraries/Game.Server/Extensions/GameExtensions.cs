using QuantumCore.API.Game.World;

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
}