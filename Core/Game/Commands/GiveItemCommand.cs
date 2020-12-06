using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("give", "Puts the given item in the inventory")]
    public static class GiveItemCommand
    {
        [CommandMethod]
        public static async void GiveMyself(IPlayerEntity player, uint itemId, int count = 1)
        {
            // todo replace item with item instance and let command manager do the lookup!
            // So we can also allow to give the item to another user
            var item = ItemManager.GetItem(itemId);
            if (item == null)
            {
                player.SendChatMessage("Item not found");
                return;
            }

            // todo migrate to plugin api style as soon as more is implemented
            if (!(player is PlayerEntity p))
            {
                return;
            }

            // Create item
            var instance = ItemManager.CreateItem(item);
            // Add item to players inventory
            if (!await p.Inventory.PlaceItem(instance))
            {
                // No space left in inventory, drop item with player name
                return;
            }
            // Store item in cache
            await instance.Persist();

            // Send item to client
            p.SendItem(instance);
        }

        [CommandMethod]
        public static void GiveAnother(IPlayerEntity player, IPlayerEntity target, uint itemId, int count = 1)
        {
            // todo replace item with item instance and let command manager do the lookup!
            // So we can also allow to give the item to another user
        }
    }
}