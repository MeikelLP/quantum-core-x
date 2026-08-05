using System.Collections.Immutable;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Utils;

namespace QuantumCore.Game.Extensions;

public static class GameExtensions
{
    extension(IEntity entity)
    {
        public ImmutableArray<IPlayerEntity> NearbyPlayers => [.. entity.NearbyEntities.OfType<IPlayerEntity>()];

        public double DistanceTo(IEntity e2)
        {
            ArgumentNullException.ThrowIfNull(e2);
            return MathUtils.Distance(entity.PositionX, entity.PositionY, e2.PositionX, e2.PositionY);
        }
    }
}