using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.Persistence;

namespace QuantumCore.Game.Commands
{
    [Command("give", "Puts the given item in the inventory")]
    public class GiveItemCommand : ICommandHandler<GiveCommandOptions>
    {
        private readonly IItemManager _itemManager;
        private readonly IItemRepository _itemRepository;
        private readonly IWorld _world;

        public GiveItemCommand(IItemManager itemManager, IItemRepository itemRepository, IWorld world)
        {
            _itemManager = itemManager;
            _itemRepository = itemRepository;
            _world = world;
        }

        public async Task ExecuteAsync(CommandContext<GiveCommandOptions> context)
        {
            var target = string.Equals(context.Arguments.Target, "$self", StringComparison.InvariantCultureIgnoreCase)
                ? context.Player
                : _world.GetPlayer(context.Arguments.Target);

            if (target is null)
            {
                context.Player.SendChatInfo("Target not found");
            }
            else
            {
                var item = _itemManager.GetItem(context.Arguments.ItemId);
                if (item == null)
                {
                    context.Player.SendChatInfo("Item not found");
                    return;
                }

                var instance = new ItemInstance
                {
                    ItemId = item.Id, Count = context.Arguments.Count, PlayerId = context.Player.Player.Id,
                };
                // Add item to players inventory
                if (!await target.Inventory.PlaceItem(instance))
                {
                    // No space left in inventory, drop item with player name
                    context.Player.SendChatInfo("No place in inventory");
                    return;
                }

                await instance.Persist(_itemRepository);

                // Send item to client
                target.SendItem(instance);
            }
        }
    }

    public class GiveCommandOptions
    {
        [Value(0)] public string Target { get; set; } = "$self";

        [Value(1)] public uint ItemId { get; set; }

        [Value(2)] public byte Count { get; set; } = 1;
    }
}
