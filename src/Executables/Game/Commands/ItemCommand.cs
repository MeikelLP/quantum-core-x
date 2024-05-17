using CommandLine;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.Caching;
using QuantumCore.Extensions;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands
{
    [Command("item", "Puts the given item in the inventory")]
    public class ItemCommand : ICommandHandler<ItemCommandOptions>
    {
        private readonly IItemManager _itemManager;
        private readonly ICacheManager _cacheManager;

        public ItemCommand(IItemManager itemManager, ICacheManager cacheManager)
        {
            _itemManager = itemManager;
            _cacheManager = cacheManager;
        }

        public async Task ExecuteAsync(CommandContext<ItemCommandOptions> context)
        {
            var item = _itemManager.GetItem(context.Arguments.ItemId);
            if (item == null)
            {
                context.Player.SendChatInfo("Item not found");
                return;
            }

            var instance = new ItemInstance {Id = Guid.NewGuid(), ItemId = item.Id, Count = context.Arguments.Count};
            if (!await context.Player.Inventory.PlaceItem(instance))
            {
                context.Player.SendChatInfo("No place in inventory");
                return;
            }

            await instance.Persist(_cacheManager);
            context.Player.SendItem(instance);
        }
    }

    public class ItemCommandOptions
    {
        [Value(0)] public uint ItemId { get; set; }

        [Value(1)] public byte Count { get; set; } = 1;
    }
}
