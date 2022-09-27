using System.Threading.Tasks;
using QuantumCore.API;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Cache;
using QuantumCore.Game.Extensions;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("give", "Puts the given item in the inventory")]
    public class GiveItemCommand
    {
        private readonly IItemManager _itemManager;
        private readonly ICacheManager _cacheManager;

        public GiveItemCommand(IItemManager itemManager, ICacheManager cacheManager)
        {
            _itemManager = itemManager;
            _cacheManager = cacheManager;
        }
        
        [CommandMethod]
        public async Task GiveMyself(IPlayerEntity player, uint itemId, byte count = 1)
        {
            await GiveAnother(player, player, itemId, count);
        }

        [CommandMethod]
        public async Task GiveAnother(IPlayerEntity player, IPlayerEntity target, uint itemId, byte count = 1)
        {
            // todo replace item with item instance and let command manager do the lookup!
            // So we can also allow to give the item to another user
            var item = _itemManager.GetItem(itemId);
            if (item == null)
            {
                await player.SendChatInfo("Item not found");
                return;
            }

            // todo migrate to plugin api style as soon as more is implemented
            if (!(target is PlayerEntity p))
            {
                return;
            }
            await p.AddItemAmountAsync(_itemManager, _cacheManager, itemId, count);
        }
    }
}