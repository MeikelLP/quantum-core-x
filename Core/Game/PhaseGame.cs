using System.Linq;
using QuantumCore.Game.Packets;
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
            
            foreach (var entity in connection.Player.NearbyEntities)
            {
                if(entity is PlayerEntity player)
                {
                    player.Connection.Send(movement);
                }
            }
        }
		
        public static void OnChat(this GameConnection connection, ChatIncoming packet)
        {
            var newMessage = connection.Player.Name + ": " + packet.Message;
            var chat = new ChatOutcoming
            {
                MessageType = (byte)ChatMessageTypes.Normal,
                Vid = connection.Player.Vid,
                Empire = 1,
                Message = newMessage
            };

            connection.Send(chat);

            foreach (var entity in connection.Player.NearbyEntities)
            {
                if (entity is PlayerEntity player)
                {
                    player.Connection.Send(chat);
                }
            }
        }
    }
}