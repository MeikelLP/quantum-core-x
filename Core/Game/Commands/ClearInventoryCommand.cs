using System.Linq;
using System.Threading.Tasks;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Extensions;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("ip", "Clears inventory and equipped items")]
    public static class ClearInventoryCommand
    {
        [CommandMethod]
        public static async Task ClearInventory(IPlayerEntity player)
        {
            if (!(player is PlayerEntity p))
            {
                return;
            }

            var items = p.Inventory.Items
                .Append(p.Inventory.EquipmentWindow.Body)
                .Append(p.Inventory.EquipmentWindow.Bracelet)
                .Append(p.Inventory.EquipmentWindow.Costume)
                .Append(p.Inventory.EquipmentWindow.Earrings)
                .Append(p.Inventory.EquipmentWindow.Hair)
                .Append(p.Inventory.EquipmentWindow.Head)
                .Append(p.Inventory.EquipmentWindow.Necklace)
                .Append(p.Inventory.EquipmentWindow.Shoes)
                .Append(p.Inventory.EquipmentWindow.Weapon)
                .Where(x => x is not null)
                .ToArray();
            foreach (var item in items)
            {
                await p.RemoveItem(item);
                await p.SendRemoveItem(item.Window, (ushort)item.Position);
                await item.Destroy();
            }

            await p.SendInventory();
        }
    }
}