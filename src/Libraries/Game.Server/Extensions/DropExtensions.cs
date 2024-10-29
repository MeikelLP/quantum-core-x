using System.Collections.Immutable;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Drops;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.Extensions;

public static class DropExtensions
{
    public static bool CanDropFor(this CommonDropEntry drop, IPlayerEntity player)
    {
        return drop.MinLevel <= player.GetPoint(EPoints.Level) &&
               drop.MaxLevel >= player.GetPoint(EPoints.Level);
    }

    public static bool CanDropFor(this LevelItemGroup drop, IPlayerEntity player)
    {
        return drop.LevelLimit <= player.GetPoint(EPoints.Level);
    }

    public static ImmutableArray<CommonDropEntry> GetPossibleCommonDropsForPlayer(this IDropProvider dropProvider,
        IPlayerEntity player)
    {
        return [..dropProvider.CommonDrops.Where(x => x.CanDropFor(player))];
    }

    public static MonsterItemGroup? GetPossibleMobDropsForPlayer(this IDropProvider dropProvider, uint monsterProtoId)
    {
        return dropProvider.GetMonsterDropsForMob(monsterProtoId);
    }

    public static ImmutableArray<LevelItemGroup> GetPossibleLevelDropsForPlayer(this IDropProvider dropProvider,
        IPlayerEntity player)
    {
        return [..dropProvider.LevelDrops.Where(x => x.CanDropFor(player))];
    }
}