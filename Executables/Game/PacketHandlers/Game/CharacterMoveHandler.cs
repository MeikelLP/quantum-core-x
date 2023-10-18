using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

public class CharacterMoveHandler : IGamePacketHandler<CharacterMove>
{
    private readonly ILogger<CharacterMoveHandler> _logger;

    public CharacterMoveHandler(ILogger<CharacterMoveHandler> logger)
    {
        _logger = logger;
    }

    public async Task ExecuteAsync(GamePacketContext<CharacterMove> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.MovementType > (int) CharacterMove.CharacterMovementType.Max &&
            ctx.Packet.MovementType != (int) CharacterMove.CharacterMovementType.Skill)
        {
            _logger.LogError("Received unknown movement type ({MovementType})", ctx.Packet.MovementType);
            ctx.Connection.Close();
            return;
        }

        _logger.LogDebug("Received movement packet with type {MovementType}", (CharacterMove.CharacterMovementType)ctx.Packet.MovementType);
        if (ctx.Packet.MovementType == (int) CharacterMove.CharacterMovementType.Move)
        {
            ctx.Connection.Player.Rotation = ctx.Packet.Rotation * 5;
            ctx.Connection.Player.Goto(ctx.Packet.PositionX, ctx.Packet.PositionY);
        }

        if (ctx.Packet.MovementType == (int) CharacterMove.CharacterMovementType.Wait)
        {
            ctx.Connection.Player.Wait(ctx.Packet.PositionX, ctx.Packet.PositionY);
        }

        var movement = new CharacterMoveOut
        {
            MovementType = ctx.Packet.MovementType,
            Argument = ctx.Packet.Argument,
            Rotation = ctx.Packet.Rotation,
            Vid = ctx.Connection.Player.Vid,
            PositionX = ctx.Packet.PositionX,
            PositionY = ctx.Packet.PositionY,
            Time = ctx.Packet.Time,
            Duration = ctx.Packet.MovementType == (int) CharacterMove.CharacterMovementType.Move
                ? ctx.Connection.Player.MovementDuration
                : 0
        };

        foreach (var entity in ctx.Connection.Player.NearbyEntities)
        {
            if(entity is PlayerEntity player)
            {
                player.Connection.Send(movement);
            }
        };
    }
}
