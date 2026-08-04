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

    public static async Task OnNpcClickAsync(uint npcId, IPlayerEntity player)
    {
        if (!NpcClickEvents.TryGetValue(npcId, out var value))
        {
            return;
        }

        var events = value.Where(e => e.Condition is null || e.Condition(player)).ToList();
        if (events.Count > 1)
        {
            // todo make sure interface IPlayerEntity is enough
            var internalQuest = player.GetQuestInstance<InternalQuest>();

            if (internalQuest is null)
            {
                return;
            }

            var selected = await internalQuest.SelectQuestAsync(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }

            await events[selected].Callback(player);

            return;
        }

        if (events.Count == 0)
        {
            return;
        }

        await events[0].Callback(player);
    }

    public static async Task OnNpcGiveAsync(uint npcId, IPlayerEntity player, ItemInstance item)
    {
        if (!NpcGiveEvents.TryGetValue(npcId, out var value))
        {
            return;
        }

        var events = value.Where(e => e.Condition is null || e.Condition(player, item)).ToList();
        if (events.Count > 1)
        {
            // todo make sure interface IPlayerEntity is enough
            var internalQuest = player.GetQuestInstance<InternalQuest>();

            if (internalQuest is null) return;

            var selected = await internalQuest.SelectQuestAsync(events.Select(e => e.Name));
            if (events[selected].Callback.Target is not Quest.Quest)
            {
                internalQuest.EndQuest();
            }

            await events[selected].Callback(player, item);

            return;
        }

        if (events.Count == 0)
        {
            return;
        }

        await events[0].Callback(player, item);
    }

    public static void RegisterNpcClickEvent(string name, uint npcId, Func<IPlayerEntity, Task> callback,
        Func<IPlayerEntity, bool>? condition = null)
    {
        if (!NpcClickEvents.TryGetValue(npcId, out var value))
        {
            value = new List<NpcClickEvent>();
            NpcClickEvents[npcId] = value;
        }

        value.Add(new NpcClickEvent
        {
            Name = name,
            NpcId = npcId,
            Callback = callback,
            Condition = condition
        });
    }

    public static void RegisterNpcGiveEvent(string name, uint npcId, Func<IPlayerEntity, ItemInstance, Task> callback,
        Func<IPlayerEntity, ItemInstance, bool>? condition = null)
    {
        if (!NpcGiveEvents.TryGetValue(npcId, out var value))
        {
            value = new List<NpcGiveEvent>();
            NpcGiveEvents[npcId] = value;
        }

        value.Add(new NpcGiveEvent
        {
            Name = name,
            NpcId = npcId,
            Callback = callback,
            Condition = condition
        });
    }
}