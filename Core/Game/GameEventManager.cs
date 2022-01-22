using System;
using System.Collections.Generic;
using System.Linq;
using QuantumCore.API.Game.World;

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

    public static void OnNpcClick(uint npcId, IPlayerEntity player)
    {
        if (!NpcClickEvents.ContainsKey(npcId))
        {
            return;
        }

        var events = NpcClickEvents[npcId].Where(e => e.Condition == null || e.Condition(player)).ToList();
        if (events.Count > 1)
        {
            // todo show quest window with selection
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