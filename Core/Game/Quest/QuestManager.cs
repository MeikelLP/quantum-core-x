using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Quest;

public class QuestManager : IQuestManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuestManager> _logger;
    private readonly Dictionary<string, Type> Quests = new();

    public QuestManager(IServiceProvider serviceProvider, ILogger<QuestManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Init()
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

    public void InitializePlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }
        
        foreach (var (id, questType) in Quests)
        {
            // todo load state
            var state = new QuestState();
            Quest quest;
            try
            {
                quest = (Quest) ActivatorUtilities.CreateInstance(_serviceProvider, questType, state, player);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to initialize quest {Id} for {Player}", id, player);
                continue;
            }
            
            quest.Init();
            p.Quests[id] = quest;
        }
    }

    public void RegisterQuest(Type questType)
    {
        var id = questType.FullName ?? Guid.NewGuid().ToString();
        if (Quests.ContainsKey(id))
        {
            _logger.LogError("Can't register quest {Type} because it's already registered or a duplicate", questType.FullName);
            return;
        }

        _logger.LogInformation("Registered quest {Id}", id);
        Quests[id] = questType;
    }
}