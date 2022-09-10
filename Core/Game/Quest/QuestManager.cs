using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using QuantumCore.API.Game.World;
using QuantumCore.Database;
using QuantumCore.Game.Quest.QuestCondition;
using QuantumCore.Game.Quest.QuestTrigger;
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

    public static async Task InitializePlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }
        
        foreach (var (id, questType) in Quests)
        {
            var state = new QuestState(p.Player.Id, id);
            await state.Load();
            
            var quest = (Quest) Activator.CreateInstance(questType, state, player);
            if (quest == null)
            {
                Log.Warning($"Failed to initialize quest {id} for {player}");
                continue;
            }
            
            quest.Init();
            InitializeTriggers(quest);
            
            p.Quests[id] = quest;
        }
    }

    public static async Task PersistPlayer(IPlayerEntity player)
    {
        if (player is not PlayerEntity p)
        {
            return;
        }

        foreach (var quest in p.Quests)
        {
            await quest.Value.State.Save();
        }
    }

    private static void InitializeTriggers(Quest quest)
    {
        var type = quest.GetType();
        foreach (var method in type.GetMethods())
        {
            // todo it's better if we only register events once and map them in here based on the player and other parameters
            
            var clickTriggers = method.GetCustomAttributes<NpcClickAttribute>();
            foreach (var clickTrigger in clickTriggers)
            {
                GameEventManager.RegisterNpcClickEvent(
                    string.IsNullOrWhiteSpace(clickTrigger.Name) ? quest.GetType().Name : clickTrigger.Name, 
                    clickTrigger.NpcId,
                    _ =>
                    {
                        CallTrigger(quest, method, clickTrigger);
                    },
                    player => player == quest.Player && CheckConditions(quest, method, player));
            }

            var giveTriggers = method.GetCustomAttributes<NpcGiveAttribute>();
            foreach (var giveTrigger in giveTriggers)
            {
                Log.Information($"Register give trigger on {giveTrigger.NpcId}");
                
                GameEventManager.RegisterNpcGiveEvent(
                    string.IsNullOrWhiteSpace(giveTrigger.Name) ? quest.GetType().Name : giveTrigger.Name, 
                    giveTrigger.NpcId,
                    (_, item) =>
                    {
                        CallTrigger(quest, method, giveTrigger, item);
                    },
                    (player, item) => player == quest.Player && CheckConditions(quest, method, player, item));
            }
            
            var levelUpTrigger = method.GetCustomAttribute<LevelUpAttribute>();
            if (levelUpTrigger != null)
            {
                GameEventManager.RegisterLevelUpEvent(
                    _ =>
                    {
                        CallTrigger(quest, method, levelUpTrigger);
                    },
                    player => player == quest.Player && CheckConditions(quest, method, player));
            }

            var killTriggers = method.GetCustomAttributes<MonsterKillAttribute>();
            foreach (var killTrigger in killTriggers)
            {
                Log.Information($"Register kill trigger on {killTrigger.MonsterId}");
                
                GameEventManager.RegisterMonsterKillEvent(
                    killTrigger.MonsterId,
                    _ =>
                    {
                        CallTrigger(quest, method, killTrigger);
                    },
                    player => player == quest.Player && CheckConditions(quest, method, player));
            }
        }
    }

    private static void CallTrigger([NotNull] Quest quest, [NotNull] MethodInfo info, Attribute trigger, Item item = null)
    {
        var parameterInfos = info.GetParameters();
        var parameters = new List<object>();
        
        if (parameterInfos.Length == 0)
        {
            info.Invoke(quest, parameters.ToArray());
            return;
        }
        
        foreach (var parameter in parameterInfos)
        {
            if (parameter.ParameterType.IsInstanceOfType(trigger))
            {
                parameters.Add(trigger);
            } else if (parameter.ParameterType == typeof(Item))
            {
                parameters.Add(item);
            }
            else
            {
                Log.Error($"Failed to invoke quest method {quest.GetType().FullName}.{info.Name}");
                return;
            }
        }

        info.Invoke(quest, parameters.ToArray());
    }

    private static bool CheckConditions(Quest quest, MemberInfo info, IPlayerEntity player, Item item = null)
    {
        Log.Debug($"Checking conditions for {info}");
        
        var conditions = info.GetCustomAttributes<Condition>();
        foreach (var condition in conditions)
        {
            if (!condition.Evaluate(quest))
            {
                Log.Debug($"Condition {condition} not true!");
                return false;
            }
        }
        
        return true;
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