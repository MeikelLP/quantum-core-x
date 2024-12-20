using QuantumCore.API.Core.Models;
using QuantumCore.API.Game;
using QuantumCore.Caching;
using QuantumCore.Game.Extensions;

namespace QuantumCore.Game.Commands
{
    [Command("ip", "Clears inventory and equipped items")]
    [Command("ipurge", "Clears inventory and equipped items")]
    public class ClearInventoryCommand : ICommandHandler
    {
        private readonly ICacheManager _cacheManager;

        public ClearInventoryCommand(ICacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task ExecuteAsync(CommandContext ctx)
        {
            var items = ctx.Player.Inventory.Items
                .Append(ctx.Player.Inventory.EquipmentWindow.Body)
                .Append(ctx.Player.Inventory.EquipmentWindow.Bracelet)
                .Append(ctx.Player.Inventory.EquipmentWindow.Costume)
                .Append(ctx.Player.Inventory.EquipmentWindow.Earrings)
                .Append(ctx.Player.Inventory.EquipmentWindow.Hair)
                .Append(ctx.Player.Inventory.EquipmentWindow.Head)
                .Append(ctx.Player.Inventory.EquipmentWindow.Necklace)
                .Append(ctx.Player.Inventory.EquipmentWindow.Shoes)
                .Append(ctx.Player.Inventory.EquipmentWindow.Weapon)
                .Where(x => x is not null)
                .Cast<ItemInstance>()
                .ToArray();
            foreach (var item in items)
            {
                ctx.Player.RemoveItem(item);
                ctx.Player.SendRemoveItem(item.Window, (ushort)item.Position);
                await item.Destroy(_cacheManager);
            }

            ctx.Player.SendInventory();
        }
    }
}
