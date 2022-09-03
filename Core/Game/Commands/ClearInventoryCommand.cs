using System.Linq;
using QuantumCore.API.Game;
using QuantumCore.API.Game.World;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.Commands
{
    [Command("ip", "Clears inventory and equipped items")]
    public static class ClearInventoryCommand
    {
        [CommandMethod]
        public static void ClearInventory(IPlayerEntity player)
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
                p.RemoveItem(item);
                p.SendRemoveItem(item.Window, (ushort)item.Position);
                item.Destroy();
            }

            p.SendInventory();
        }
    }
}