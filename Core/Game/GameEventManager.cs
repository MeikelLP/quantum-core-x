using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public Func<IPlayerEntity, Task> Callback { get; set; }
        public Func<IPlayerEntity, bool> Condition { get; set; }
    }

    private struct NpcGiveEvent
    {
        public string Name { get; set; }
        public uint NpcId { get; set; }
        public Func<IPlayerEntity, Item, Task> Callback { get; set; }
        public Func<IPlayerEntity, Item, bool> Condition { get; set; }
    }

    private static readonly Dictionary<uint, List<NpcClickEvent>> NpcClickEvents = new();
    private static readonly Dictionary<uint, List<NpcGiveEvent>> NpcGiveEvents = new();

    public static async Task OnNpcClick(uint npcId, IPlayerEntity player)
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

    public static async Task OnNpcGive(uint npcId, IPlayerEntity player, Item item)
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
    
    public static void RegisterNpcClickEvent(string name, uint npcId, Func<IPlayerEntity, Task> callback, 
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

    public static void RegisterNpcGiveEvent(string name, uint npcId, Func<IPlayerEntity, Item, Task> callback,
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
}