using System.Diagnostics;
using QuantumCore.API;
using QuantumCore.API.Core.Models;
using QuantumCore.API.Extensions;
using QuantumCore.API.Game.Types;
using QuantumCore.API.Game.Types.Entities;
using QuantumCore.API.Game.Types.Items;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.World;
using QuantumCore.API.Packets;

// ReSharper disable once CheckNamespace
namespace QuantumCore.Game;

public static class PlayerExtensions
{
    public static EPlayerClass GetClass(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WARRIOR_MALE => EPlayerClass.WARRIOR,
            EPlayerClassGendered.NINJA_FEMALE => EPlayerClass.NINJA,
            EPlayerClassGendered.SURA_MALE => EPlayerClass.SURA,
            EPlayerClassGendered.SHAMAN_FEMALE => EPlayerClass.SHAMAN,
            EPlayerClassGendered.WARRIOR_FEMALE => EPlayerClass.WARRIOR,
            EPlayerClassGendered.NINJA_MALE => EPlayerClass.NINJA,
            EPlayerClassGendered.SURA_FEMALE => EPlayerClass.SURA,
            EPlayerClassGendered.SHAMAN_MALE => EPlayerClass.SHAMAN,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }

    public static EPlayerGender GetGender(this EPlayerClassGendered playerClass)
    {
        return playerClass switch
        {
            EPlayerClassGendered.WARRIOR_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.NINJA_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.SURA_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.SHAMAN_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.WARRIOR_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.NINJA_MALE => EPlayerGender.MALE,
            EPlayerClassGendered.SURA_FEMALE => EPlayerGender.FEMALE,
            EPlayerClassGendered.SHAMAN_MALE => EPlayerGender.MALE,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), playerClass, null)
        };
    }

    extension(IPlayerEntity player)
    {
        public void SendBasicData()
        {
            var details = new CharacterDetails
            {
                Vid = player.Vid,
                Name = player.Name,
                Class = player.Player.PlayerClass,
                PositionX = player.PositionX,
                PositionY = player.PositionY,
                Empire = player.Empire,
                SkillGroup = player.Player.SkillGroup
            };
            player.Connection.Send(details);
        }

        public void SendPoints()
        {
            var points = new CharacterPoints();
            for (var i = 0; i < points.Points.Length; i++)
            {
                points.Points[i] = player.GetPoint((EPoint)i);
            }

            player.Connection.Send(points);
        }

        public void SendInventory()
        {
            foreach (var item in player.Inventory.Items)
            {
                player.SendItem(item);
            }

            player.Inventory.EquipmentWindow.Send(player);
        }

        public void SendItem(ItemInstance item)
        {
            ArgumentNullException.ThrowIfNull(item);
            Debug.Assert(item.PlayerId == player.Player.Id);

            var p = new SetItem
            {
                Window = item.Window, Position = (ushort)item.Position, ItemId = item.ItemId, Count = item.Count
            };
            player.Connection.Send(p);
        }

        public void SendRemoveItem(WindowType window, ushort position)
        {
            player.Connection.Send(new SetItem { Window = window, Position = position, ItemId = 0, Count = 0 });
        }

        public void SendCharacter(IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);
            connection.Send(new SpawnCharacter
            {
                Vid = player.Vid,
                CharacterType = EEntityType.PLAYER,
                Angle = 0,
                PositionX = player.PositionX,
                PositionY = player.PositionY,
                Class = (ushort)player.Player.PlayerClass,
                MoveSpeed = player.MovementSpeed,
                AttackSpeed = player.AttackSpeed
            });
        }

        public void SendCharacterAdditional(IConnection connection)
        {
            ArgumentNullException.ThrowIfNull(connection);
            connection.Send(new CharacterInfo
            {
                Vid = player.Vid,
                Name = player.Player.Name,
                Empire = player.Player.Empire,
                Level = player.Player.Level,
                GuildId = player.Player.GuildId ?? 0,
                Parts =
                [
                    (ushort)(player.Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort)(player.Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort)player.Inventory.EquipmentWindow.Hair.GetHairPartOffsetForClient(player.Player.PlayerClass
                        .GetClass())
                ]
            });
        }

        public void SendCharacterUpdate()
        {
            var packet = new CharacterUpdate
            {
                Vid = player.Vid,
                Parts =
                [
                    (ushort)(player.Inventory.EquipmentWindow.Body?.ItemId ?? 0),
                    (ushort)(player.Inventory.EquipmentWindow.Weapon?.ItemId ?? 0), 0,
                    (ushort)player.Inventory.EquipmentWindow.Hair.GetHairPartOffsetForClient(player.Player.PlayerClass
                        .GetClass())
                ],
                MoveSpeed = player.MovementSpeed,
                AttackSpeed = player.AttackSpeed,
                GuildId = player.Player.GuildId ?? 0
            };

            player.Connection.Send(packet);

            foreach (var entity in player.NearbyEntities)
            {
                if (entity is IPlayerEntity p)
                {
                    p.Connection.Send(packet);
                }
            }
        }

        public void SendChatMessage(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageType.NORMAL, Vid = player.Vid, Empire = player.Empire, Message = message
            };
            player.Connection.Send(chat);
        }

        public void SendChatCommand(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageType.COMMAND, Vid = 0, Empire = player.Empire, Message = message
            };
            player.Connection.Send(chat);
        }

        public void SendChatInfo(string message)
        {
            var chat = new ChatOutcoming
            {
                MessageType = ChatMessageType.INFO, Vid = 0, Empire = player.Empire, Message = message
            };
            player.Connection.Send(chat);
        }

        public void SendTarget()
        {
            if (player.Target is not null)
            {
                var packet = new SetTarget
                {
                    TargetVid = player.Target.Vid,
                    Percentage = player.Target.HealthPercentage
                };

                player.Connection.Send(packet);
            }
        }
    }
}