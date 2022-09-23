using System.Linq;
using System.Threading.Tasks;
using QuantumCore.API.Game.World;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Packets;
using QuantumCore.Game.Packets.Quest;
using QuantumCore.Game.Packets.QuickBar;
using QuantumCore.Game.Packets.Shop;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.Quest;
using QuantumCore.Game.World.Entities;
using Serilog;
using Serilog.Core;

namespace QuantumCore.Game
{
    public static class PhaseGame
    {
        [Listener(typeof(CharacterMove))]
        public static async Task OnCharacterMove(this GameConnection connection, CharacterMove packet)
        {
            if (packet.MovementType > (int) CharacterMove.CharacterMovementType.Max &&
                packet.MovementType != (int) CharacterMove.CharacterMovementType.Skill)
            {
                Log.Error($"Received unknown movement type ({packet.MovementType})");
                connection.Close();
                return;
            }
            
            Log.Debug($"Received movement packet with type {(CharacterMove.CharacterMovementType)packet.MovementType}");
            if (packet.MovementType == (int) CharacterMove.CharacterMovementType.Move)
            {
                connection.Player.Rotation = packet.Rotation * 5;
                await connection.Player.Goto(packet.PositionX, packet.PositionY);
            }
            
            if (packet.MovementType == (int) CharacterMove.CharacterMovementType.Wait)
            {
                connection.Player.Wait(packet.PositionX, packet.PositionY);
            }

            var movement = new CharacterMoveOut
            {
                MovementType = packet.MovementType,
                Argument = packet.Argument,
                Rotation = packet.Rotation,
                Vid = connection.Player.Vid,
                PositionX = packet.PositionX,
                PositionY = packet.PositionY,
                Time = packet.Time,
                Duration = packet.MovementType == (int) CharacterMove.CharacterMovementType.Move
                    ? connection.Player.MovementDuration
                    : 0
            };
            
            await connection.Player.ForEachNearbyEntity(async entity =>
            {
                if(entity is PlayerEntity player)
                {
                    await player.Connection.Send(movement);
                }
            });
        }

        [Listener(typeof(ChatIncoming))]
        public static async Task OnChat(this GameConnection connection, ICommandManager commandManager, ChatIncoming packet)
        {
            if (packet.MessageType == ChatMessageTypes.Normal)
            {
                if (packet.Message.StartsWith('/'))
                {
                    await commandManager.Handle(connection, packet.Message);
                }
                else
                {
                    var message = connection.Player.Name + ": " + packet.Message;

                    await ChatManager.Talk(connection.Player, message);
                }
            }

            if (packet.MessageType == ChatMessageTypes.Shout)
            {
                // todo check 15 seconds cooldown
                var message = connection.Player.Name + ": " + packet.Message;
                
                await ChatManager.Shout(message);
            }
        }

        [Listener(typeof(ItemMove))]
        public static async Task OnItemMove(this GameConnection connection, ItemMove packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }
            
            Log.Debug($"Move item from {packet.FromWindow},{packet.FromPosition} to {packet.ToWindow},{packet.ToPosition}");

            // Get moved item
            var item = player.GetItem(packet.FromWindow, packet.FromPosition);
            if (item == null)
            {
                Log.Debug($"Moved item not found!");
                return;
            }

            // Check if target space is available
            if (player.IsSpaceAvailable(item, packet.ToWindow, packet.ToPosition))
            {
                // remove from old space
                await player.RemoveItem(item);
                
                // place item
                await player.SetItem(item, packet.ToWindow, packet.ToPosition);

                // send item movement to client
                await player.SendRemoveItem(packet.FromWindow, packet.FromPosition);
                await player.SendItem(item);
            }
        }
        
        // TODO this will not work as IItemManager cannot be injected here (yet)
        [Listener(typeof(ItemUse))]
        public static async Task OnItemUse(this GameConnection connection, IItemManager itemManager, ItemUse packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            Log.Debug($"Use item {packet.Window},{packet.Position}");

            var item = player.GetItem(packet.Window, packet.Position);
            if (item == null)
            {
                Log.Debug($"Used item not found!");
                return;
            }

            var itemProto = itemManager.GetItem(item.ItemId);
            if (itemProto == null)
            {
                Log.Debug($"Cannot find item proto {item.ItemId}");
                return;
            }

            if (packet.Window == (byte) WindowType.Inventory && packet.Position >= player.Inventory.Size)
            {
                await player.RemoveItem(item);
                if (await player.Inventory.PlaceItem(item))
                {
                    await player.SendRemoveItem(packet.Window, packet.Position);
                    await player.SendItem(item);
                    await player.SendCharacterUpdate();
                }
                else
                {
                    await player.SetItem(item, packet.Window, packet.Position);
                    await player.SendChatInfo("Cannot unequip item if the inventory is full");
                }
            }
            else if (player.IsEquippable(item))
            {
                var wearSlot = player.Inventory.EquipmentWindow.GetWearSlot(itemManager, item);

                if (wearSlot <= ushort.MaxValue)
                {
                    var item2 = player.Inventory.EquipmentWindow.GetItem((ushort)wearSlot);

                    if (item2 != null)
                    {
                        await player.RemoveItem(item);
                        await player.RemoveItem(item2);
                        if (await player.Inventory.PlaceItem(item2))
                        {
                            await player.SendRemoveItem(packet.Window, (ushort)wearSlot);
                            await player.SendRemoveItem(packet.Window, packet.Position);
                            await player.SetItem(item, packet.Window, (ushort)wearSlot);
                            await player.SetItem(item2, packet.Window, packet.Position);
                            await player.SendItem(item);
                            await player.SendItem(item2);
                        }
                        else
                        {
                            await player.SetItem(item, packet.Window, packet.Position);
                            await player.SetItem(item2, packet.Window, (ushort)wearSlot);
                            await player.SendChatInfo("Cannot swap item if the inventory is full");
                        }
                    }
                    else
                    {
                        await player.RemoveItem(item);
                        await player.SetItem(item, (byte) WindowType.Inventory, (ushort)wearSlot);
                        await player.SendRemoveItem(packet.Window, packet.Position);
                        await player.SendItem(item);
                    }
                }
            }
        }

        [Listener(typeof(ItemDrop))]
        public static async Task OnItemDrop(this GameConnection connection, ItemDrop packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            if (packet.Gold > 0)
            {
                // We're dropping gold...
                await player.DropGold(packet.Gold);
            }
            else
            {
                // We're dropping an item...
                var item = player.GetItem(packet.Window, packet.Position);
                if (item == null)
                {
                    return; // Item slot is empty
                }

                await player.DropItem(item, packet.Count);
            }
        }

        [Listener(typeof(ItemPickup))]
        public static async Task OnItemPickup(this GameConnection connection, ItemPickup packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            var entity = player.Map.GetEntity(packet.Vid);
            if (entity is not GroundItem groundItem)
            {
                // we can only pick up ground items
                return;
            }

            await player.Pickup(groundItem);
        }

        [Listener(typeof(ItemGive))]
        public static async Task OnItemGive(this GameConnection connection, ItemGive packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            var entity = player.Map.GetEntity(packet.TargetVid);
            if (entity == null)
            {
                Log.Debug("Ignore item give to non existing entity");
                return;
            }

            var item = player.GetItem(packet.Window, packet.Position);
            if (item == null)
            {
                return;
            }
            
            Log.Information($"Item give to {entity}");
            await GameEventManager.OnNpcGive(entity.EntityClass, player, item);
        }

        [Listener(typeof(TargetChange))]
        public static async Task OnTargetChange(this GameConnection connection, TargetChange packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                Log.Warning("Target Change without having a player instance");
                connection.Close();
                return;
            }

            var entity = player.Map.GetEntity(packet.TargetVid);
            if (entity == null)
            {
                return;
            }

            player.Target?.TargetedBy.Remove(player);
            player.Target = entity;
            entity.TargetedBy.Add(player);
            await player.SendTarget();
        }

        [Listener(typeof(Attack))]
        public static async Task OnAttack(this GameConnection connection, Attack packet)
        {
            var attacker = connection.Player;
            if (attacker == null)
            {
                Log.Warning("Attack without having a player instance");
                connection.Close();
                return;
            }
            
            var entity = attacker.Map.GetEntity(packet.Vid);
            if (entity == null)
            {
                return;
            }
            
            Log.Debug($"Attack from {attacker.Name} with type {packet.AttackType} target {packet.Vid}");

            await attacker.Attack(entity, 0);
        }

        [Listener(typeof(ClickNpc))]
        public static async Task OnClickNpc(this GameConnection connection, ClickNpc packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            var entity = player.Map.GetEntity(packet.Vid);
            if (entity == null)
            {
                connection.Close();
                return;
            }
            
            await GameEventManager.OnNpcClick(entity.EntityClass, player);
        }

        [Listener(typeof(ShopClose))]
        public static Task OnShopClose(this GameConnection connection, ShopClose packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return Task.CompletedTask;
            }
            
            player.Shop?.Close(player);

            return Task.CompletedTask;
        }

        [Listener(typeof(ShopBuy))]
        public static Task OnShopBuy(this GameConnection connection, ShopBuy packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();

                return Task.CompletedTask;
            }

            player.Shop?.Buy(player, packet.Position, packet.Count);

            return Task.CompletedTask;
        }

        [Listener(typeof(ShopSell))]
        public static Task OnShopSell(this GameConnection connection, ShopSell packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return Task.CompletedTask;
            }

            player.Shop?.Sell(player, packet.Position);

            return Task.CompletedTask;
        }

        [Listener(typeof(QuickBarAdd))]
        public static async Task OnQuickBarAdd(this GameConnection connection, QuickBarAdd packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            await player.QuickSlotBar.Add(packet.Position, packet.Slot);
        }
        
        [Listener(typeof(QuickBarRemove))]
        public static async Task OnQuickBarRemove(this GameConnection connection, QuickBarRemove packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            await player.QuickSlotBar.Remove(packet.Position);
        }
        
        [Listener(typeof(QuickBarSwap))]
        public static async Task OnQuickBarSwap(this GameConnection connection, QuickBarSwap packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return;
            }

            await player.QuickSlotBar.Swap(packet.Position1, packet.Position2);
        }

        [Listener(typeof(QuestAnswer))]
        public static Task OnQuestAnswer(this GameConnection connection, QuestAnswer packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                connection.Close();
                return Task.CompletedTask;
            }
            
            Log.Information($"Quest answer: {packet.Answer}");
            player.CurrentQuest?.Answer(packet.Answer);

            return Task.CompletedTask;
        }
    }
}