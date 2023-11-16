using QuantumCore.API.Core.Models;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Quest;

namespace QuantumCore.Game;

public static class GameEventManager
{
    private struct NpcClickEvent
    {
        public string Name { get; set; }
        public uint NpcId { get; set; }
        public Func<IPlayerEntity, Task> Callback { get; set; }
        public Func<IPlayerEntity, bool>? Condition { get; set; }
    }

    private struct NpcGiveEvent
    {
        public string Name { get; set; }
        public uint NpcId { get; set; }
        public Func<IPlayerEntity, ItemInstance, Task> Callback { get; set; }
        public Func<IPlayerEntity, ItemInstance, bool>? Condition { get; set; }
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
            var internalQuest = player.GetQuestInstance<InternalQuest>();

            if (internalQuest is null)
            {
                return;
            }

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }
            await events[selected].Callback(player);

            return;
        }

        if (!events.Any())
        {
            return;
        }

        await events[0].Callback(player);
    }

    public static async Task OnNpcGive(uint npcId, IPlayerEntity player, ItemInstance item)
    {
        if (!NpcGiveEvents.ContainsKey(npcId))
        {
            return;
        }

        var events = NpcGiveEvents[npcId].Where(e => e.Condition == null || e.Condition(player, item)).ToList();
        if (events.Count > 1)
        {
            // todo make sure interface IPlayerEntity is enough
            var internalQuest = player.GetQuestInstance<InternalQuest>();

            if (internalQuest == null) return;

            var selected = await internalQuest.SelectQuest(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }
            await events[selected].Callback(player, item);

            return;
        }

        if (!events.Any())
        {
            return;
        }

        await events[0].Callback(player, item);
    }

    public static void RegisterNpcClickEvent(string name, uint npcId, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
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

    public static void RegisterNpcGiveEvent(string name, uint npcId, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null)
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
