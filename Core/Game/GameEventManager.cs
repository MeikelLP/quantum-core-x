using System;
using System.Collections.Generic;
using System.Linq;
using QuantumCore.API.Game.World;
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

    private static readonly Dictionary<uint, List<NpcClickEvent>> NpcClickEvents = new();

    public static async void OnNpcClick(uint npcId, IPlayerEntity player)
    {
        if (!NpcClickEvents.ContainsKey(npcId))
        {
            return;
        }

        var events = NpcClickEvents[npcId].Where(e => e.Condition == null || e.Condition(player)).ToList();
        if (events.Count > 1)
        {
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
}