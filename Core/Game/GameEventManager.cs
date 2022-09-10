using System;
using System.Collections.Generic;
using System.Linq;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.Quest;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game;

public static class GameEventManager
{
    private struct NpcClickEvent
    {
        public string Name { get; set; }
        public uint NpcId { get; set; }
        public Action<IPlayerEntity> Callback { get; set; }
        public Func<IPlayerEntity, bool> Condition { get; set; }
    }

    private struct NpcGiveEvent
    {
        public string Name { get; set; }
        public uint NpcId { get; set; }
        public Action<IPlayerEntity, Item> Callback { get; set; }
        public Func<IPlayerEntity, Item, bool> Condition { get; set; }
    }

    private struct LevelUpEvent
    {
        public Action<IPlayerEntity> Callback { get; set; }
        public Func<IPlayerEntity, bool> Condition { get; set; }
    }

    private struct MonsterKillEvent
    {
        public uint MonsterId { get; set; }
        public Action<IPlayerEntity> Callback { get; set; }
        public Func<IPlayerEntity, bool> Condition { get; set; }
    }

    private static readonly Dictionary<uint, List<NpcClickEvent>> NpcClickEvents = new();
    private static readonly Dictionary<uint, List<NpcGiveEvent>> NpcGiveEvents = new();
    private static readonly List<LevelUpEvent> LevelUpEvents = new();
    private static readonly Dictionary<uint, List<MonsterKillEvent>> MonsterKillEvents = new();

    public static async void OnNpcClick(uint npcId, IPlayerEntity player)
    {
        if (!NpcClickEvents.ContainsKey(npcId))
        {
            return;
        }

        var events = NpcClickEvents[npcId].Where(e => e.Condition == null || e.Condition(player)).ToList();
        if (events.Count > 1)
        {
            // todo make sure interface IPlayerEntity is enough
            var p = player as PlayerEntity;
            var internalQuest = p.GetQuestInstance<InternalQuest>();

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }
            events[selected].Callback(player);
            
            return;
        }

        if (!events.Any())
        {
            return; 
        }

        events[0].Callback(player);
    }

    public static async void OnNpcGive(uint npcId, IPlayerEntity player, Item item)
    {
        if (!NpcGiveEvents.ContainsKey(npcId))
        {
            return;
        }

        var events = NpcGiveEvents[npcId].Where(e => e.Condition == null || e.Condition(player, item)).ToList();
        if (events.Count > 1)
        {
            // todo make sure interface IPlayerEntity is enough
            var p = player as PlayerEntity;
            var internalQuest = p.GetQuestInstance<InternalQuest>();

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }
            events[selected].Callback(player, item);
            
            return;
        }
        
        if (!events.Any())
        {
            return; 
        }

        events[0].Callback(player, item);
    }

    public static void OnLevelUp(IPlayerEntity player)
    {
        foreach (var evt in LevelUpEvents)
        {
            if (evt.Condition == null || evt.Condition(player))
            {
                evt.Callback(player);
            }
        }
    }

    public static void OnMonsterKill(IPlayerEntity player, uint monsterId)
    {
        if (!MonsterKillEvents.ContainsKey(monsterId))
        {
            return;
        }

        foreach (var evt in MonsterKillEvents[monsterId])
        {
            if (evt.Condition == null || evt.Condition(player))
            {
                evt.Callback(player);
            }
        }
    }
    
    public static void RegisterNpcClickEvent(string name, uint npcId, Action<IPlayerEntity> callback, 
        Func<IPlayerEntity, bool> condition = null)
    {
        if (!NpcClickEvents.ContainsKey(npcId))
        {
            NpcClickEvents[npcId] = new List<NpcClickEvent>();
        }
        
        NpcClickEvents[npcId].Add(new NpcClickEvent {
            Name = name,
            NpcId = npcId,
            Callback = callback,
            Condition = condition
        });
    }

    public static void RegisterNpcGiveEvent(string name, uint npcId, Action<IPlayerEntity, Item> callback,
        Func<IPlayerEntity, Item, bool> condition = null)
    {
        if (!NpcGiveEvents.ContainsKey(npcId))
        {
            NpcGiveEvents[npcId] = new List<NpcGiveEvent>();
        }
        
        NpcGiveEvents[npcId].Add(new NpcGiveEvent {
            Name = name,
            NpcId = npcId,
            Callback = callback,
            Condition = condition
        });
    }

    public static void RegisterLevelUpEvent(Action<IPlayerEntity> callback, Func<IPlayerEntity, bool> condition = null)
    {
        LevelUpEvents.Add(new LevelUpEvent {
            Callback = callback,
            Condition = condition
        });
    }

    public static void RegisterMonsterKillEvent(uint monsterId, Action<IPlayerEntity> callback,
        Func<IPlayerEntity, bool> condition = null)
    {
        if (!MonsterKillEvents.ContainsKey(monsterId))
        {
            MonsterKillEvents[monsterId] = new List<MonsterKillEvent>();
        }
        
        MonsterKillEvents[monsterId].Add(new MonsterKillEvent {
            MonsterId = monsterId,
            Callback = callback,
            Condition = condition
        });
    }
}