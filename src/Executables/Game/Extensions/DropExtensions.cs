using System.Collections.Immutable;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Services;

namespace QuantumCore.Game.Extensions;

public static class DropExtensions
{
    public static bool CanDropFor(this CommonDropEntry drop, IPlayerEntity player)
    {
        return drop.MinLevel <= player.GetPoint(EPoints.Level) &&
               drop.MaxLevel >= player.GetPoint(EPoints.Level);
    }

    public static bool CanDropFor(this MonsterDropEntry drop, IPlayerEntity player)
    {
        return drop.MinLevel <= player.GetPoint(EPoints.Level);
    }

    public static ImmutableArray<CommonDropEntry> GetPossibleCommonDropsForPlayer(this IDropProvider dropProvider,
        IPlayerEntity player)
    {
        return [..dropProvider.CommonDrops.Where(x => x.CanDropFor(player))];
    }

    public static ImmutableArray<MonsterDropEntry> GetPossibleMobDropsForPlayer(this IDropProvider dropProvider,
        IPlayerEntity player, uint monsterProtoId)
    {
        return [..dropProvider.GetDropsForMob(monsterProtoId).Where(x => x.CanDropFor(player))];
    }
}
