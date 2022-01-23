using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.Quest;

public static class QuestManager
{
    private static readonly Dictionary<string, Type> Quests = new();

    public static void Init()
    {
        // Scan for all available quests
        var assembly = Assembly.GetAssembly(typeof(QuestManager));
        if (assembly == null)
        {
            return;
        }
        
        foreach (var questType in assembly.GetTypes().Where(type => type.GetCustomAttribute<QuestAttribute>() != null))
        {
            RegisterQuest(questType);
        }
    }

    public static void InitializePlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }
        
        foreach (var (id, questType) in Quests)
        {
            // todo load state
            var state = new QuestState();
            var quest = (Quest) Activator.CreateInstance(questType, state, player);
            if (quest == null)
            {
                Log.Warning($"Failed to initialize quest {id} for {player}");
                continue;
            }
            
            quest.Init();
            p.Quests[id] = quest;
        }
    }

    public static void RegisterQuest(Type questType)
    {
        var id = questType.FullName ?? Guid.NewGuid().ToString();
        if (Quests.ContainsKey(id))
        {
            Log.Error($"Can't register quest {questType.FullName} because it's already registered or a duplicate");
            return;
        }

        Log.Information($"Registered quest {id}");
        Quests[id] = questType;
    }
}