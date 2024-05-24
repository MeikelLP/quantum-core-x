using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(CharacterMove))]
public class CharacterMoveHandler
{
    private readonly ILogger<CharacterMoveHandler> _logger;

    public CharacterMoveHandler(ILogger<CharacterMoveHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, CharacterMove packet)
    {
        if (packet.MovementType > CharacterMovementType.Max &&
            packet.MovementType != CharacterMovementType.Skill)
        {
            _logger.LogError("Received unknown movement type ({MovementType})", packet.MovementType);
            ctx.Connection.Close();
            return;
        }

        if (ctx.Connection.Player is null)
        {
            _logger.LogCritical("Cannot move player that does not exist. This is a programmatic failure");
            ctx.Connection.Close();
            return;
        }

        _logger.LogDebug("Received movement packet with type {MovementType}", packet.MovementType);
        if (packet.MovementType == CharacterMovementType.Move)
        {
            ctx.Connection.Player.Rotation = packet.Rotation * 5;
            ctx.Connection.Player.Goto(packet.PositionX, packet.PositionY);
        }

        if (packet.MovementType == CharacterMovementType.Wait)
        {
            ctx.Connection.Player.Wait(packet.PositionX, packet.PositionY);
        }

        var movement = new CharacterMoveOut(
            packet.MovementType,
            packet.Argument,
            packet.Rotation,
            ctx.Connection.Player.Vid,
            packet.PositionX,
            packet.PositionY,
            packet.Time,
            packet.MovementType == CharacterMovementType.Move
                ? ctx.Connection.Player.MovementDuration
                : 0
        );

        foreach (var entity in ctx.Connection.Player.NearbyEntities)
        {
            if (entity is PlayerEntity player)
            {
                player.Connection.Send(movement);
            }
        }
    }
}