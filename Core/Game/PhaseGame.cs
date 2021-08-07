using System.Linq;
using QuantumCore.Game.Commands;
using QuantumCore.Game.Packets;
using QuantumCore.Game.PlayerUtils;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game
{
    public static class PhaseGame
    {
        public static void OnCharacterMove(this GameConnection connection, CharacterMove packet)
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
                connection.Player.Goto(packet.PositionX, packet.PositionY);
            }
            
            if (packet.MovementType == (int) CharacterMove.CharacterMovementType.Wait)
            {
                // todo: Verify position possibility
                connection.Player.PositionX = packet.PositionX;
                connection.Player.PositionY = packet.PositionY;
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
            
            connection.Player.ForEachNearbyEntity(entity =>
            {
                if(entity is PlayerEntity player)
                {
                    player.Connection.Send(movement);
                }
            });
        }
		
        public static void OnChat(this GameConnection connection, ChatIncoming packet)
        {
            if (packet.MessageType == ChatMessageTypes.Normal)
            {
                if (packet.Message.StartsWith('/'))
                {
                    CommandManager.Handle(connection, packet.Message);
                }
                else
                {
                    var newMessage = connection.Player.Name + ": " + packet.Message;
                    var chat = new ChatOutcoming
                    {
                        MessageType = ChatMessageTypes.Normal,
                        Vid = connection.Player.Vid,
                        Empire = 1,
                        Message = newMessage
                    };

                    connection.Send(chat);

                    connection.Player.ForEachNearbyEntity(entity =>
                    {
                        if (entity is PlayerEntity player)
                        {
                            player.Connection.Send(chat);
                        }
                    });
                }
            }
        }

        public static async void OnItemMove(this GameConnection connection, ItemMove packet)
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
                player.RemoveItem(item);
                
                // place item
                await player.SetItem(item, packet.ToWindow, packet.ToPosition);

                // send item movement to client
                player.SendRemoveItem(packet.FromWindow, packet.FromPosition);
                player.SendItem(item);
            }
        }
        public static async void OnItemUse(this GameConnection connection, ItemUse packet)
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

            var itemProto = ItemManager.GetItem(item.ItemId);
            if (itemProto == null)
            {
                Log.Debug($"Cannot find item proto {item.ItemId}");
                return;
            }

            if (packet.Window == (byte) WindowType.Inventory && packet.Position >= player.Inventory.Size)
            {
                player.RemoveItem(item);
                if (await player.Inventory.PlaceItem(item))
                {
                    player.SendRemoveItem(packet.Window, packet.Position);
                    player.SendItem(item);
                    player.SendCharacterUpdate();
                }
                else
                {
                    await player.SetItem(item, packet.Window, packet.Position);
                    player.SendChatInfo("Cannot unequip item if the inventory is full");
                }
            }
            else if (player.IsEquippable(item))
            {
                var wearSlot = player.Inventory.EquipmentWindow.GetWearSlot(item);

                if (wearSlot <= ushort.MaxValue)
                {
                    var item2 = player.Inventory.EquipmentWindow.GetItem((ushort)wearSlot);

                    if (item2 != null)
                    {
                        player.RemoveItem(item);
                        player.RemoveItem(item2);
                        if (await player.Inventory.PlaceItem(item2))
                        {
                            player.SendRemoveItem(packet.Window, (ushort)wearSlot);
                            player.SendRemoveItem(packet.Window, packet.Position);
                            await player.SetItem(item, packet.Window, (ushort)wearSlot);
                            await player.SetItem(item2, packet.Window, packet.Position);
                            player.SendItem(item);
                            player.SendItem(item2);
                        }
                        else
                        {
                            await player.SetItem(item, packet.Window, packet.Position);
                            await player.SetItem(item2, packet.Window, (ushort)wearSlot);
                            player.SendChatInfo("Cannot swap item if the inventory is full");
                        }
                    }
                    else
                    {
                        player.RemoveItem(item);
                        await player.SetItem(item, (byte) WindowType.Inventory, (ushort)wearSlot);
                        player.SendRemoveItem(packet.Window, packet.Position);
                        player.SendItem(item);
                    }
                }
            }
        }

        public static async void OnTargetChange(this GameConnection connection, TargetChange packet)
        {
            var player = connection.Player;
            if (player == null)
            {
                Log.Debug("Target Change without having a player instance");
                connection.Close();
                return;
            }

            var entity = player.Map.GetEntity(packet.TargetVid);
            if (entity == null)
            {
                return;
            }

            player.Target = entity;
            player.SendTarget();
        }
    }
}