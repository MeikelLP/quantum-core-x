using System.Collections.Immutable;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Drops;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Services;

public record struct CommonDropEntry(byte MinLevel, byte MaxLevel, uint ItemProtoId, float Chance);

public record struct EtcItemDropEntry(uint ItemProtoId, float Multiplier);

public interface IDropProvider
{
    MonsterItemGroup? GetMonsterDropsForMob(uint monsterProtoId);
    DropItemGroup? GetDropItemsGroupForMob(uint monsterProtoId);

    ImmutableArray<EtcItemDropEntry> EtcDrops { get; }

    ImmutableArray<CommonDropEntry> CommonDrops { get; }

    ImmutableArray<LevelItemGroup> LevelDrops { get; }
    (int deltaPercentage, int dropRange) CalculateDropPercentages(IPlayerEntity player, MonsterEntity monster);

    ImmutableArray<ItemInstance> CalculateCommonDropItems(IPlayerEntity player, MonsterEntity monster, int delta,
        int range);

    ImmutableArray<ItemInstance> CalculateDropItemGroupItems(MonsterEntity monster, int delta, int range);

    ImmutableArray<ItemInstance>
        CalculateMobDropItemGroupItems(IPlayerEntity player, MonsterEntity monster, int delta, int range);

    ImmutableArray<ItemInstance> CalculateLevelDropItems(IPlayerEntity player, MonsterEntity monster, int delta,
        int range);

    ImmutableArray<ItemInstance> CalculateEtcDropItems(MonsterEntity monster, int delta, int range);
    ImmutableArray<ItemInstance> CalculateMetinDropItems(MonsterEntity monsterEntity, int delta, int range);
}