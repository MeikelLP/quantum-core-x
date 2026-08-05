using System.Collections.Immutable;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Drops;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.Extensions;

public static class DropExtensions
{
    public static bool CanDropFor(this CommonDropEntry drop, IPlayerEntity player)
    {
        ArgumentNullException.ThrowIfNull(player);
        return drop.MinLevel <= player.GetPoint(EPoint.LEVEL) &&
               drop.MaxLevel >= player.GetPoint(EPoint.LEVEL);
    }

    public static bool CanDropFor(this LevelItemGroup drop, IPlayerEntity player)
    {
        ArgumentNullException.ThrowIfNull(drop);
        ArgumentNullException.ThrowIfNull(player);
        return drop.LevelLimit <= player.GetPoint(EPoint.LEVEL);
    }

    extension(IDropProvider dropProvider)
    {
        public ImmutableArray<CommonDropEntry> GetPossibleCommonDropsForPlayer(IPlayerEntity player)
        {
            return [.. dropProvider.CommonDrops.Where(x => x.CanDropFor(player))];
        }

        public MonsterItemGroup? GetPossibleMobDropsForPlayer(uint monsterProtoId)
        {
            return dropProvider.GetMonsterDropsForMob(monsterProtoId);
        }

        public ImmutableArray<LevelItemGroup> GetPossibleLevelDropsForPlayer(IPlayerEntity player)
        {
            return [.. dropProvider.LevelDrops.Where(x => x.CanDropFor(player))];
        }
    }
}