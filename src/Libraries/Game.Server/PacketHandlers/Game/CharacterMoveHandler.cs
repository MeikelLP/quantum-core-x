using EnumsNET;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types.Players;
using QuantumCore.API.Game.Types.Skills;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

public class CharacterMoveHandler : IGamePacketHandler<CharacterMove>
{
    private readonly ILogger<CharacterMoveHandler> _logger;

    private const byte MASK_SKILL_MOTION = (byte)CharacterMovementType.SKILL_FLAG - 1;

    public CharacterMoveHandler(ILogger<CharacterMoveHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<CharacterMove> ctx, CancellationToken token = default)
    {
        if (!ctx.Packet.MovementType.IsDefined() && !ctx.Packet.MovementType.HasAnyFlags(CharacterMovementType.SKILL_FLAG))
        {
            _logger.LogError("Received unknown movement type ({MovementType})", ctx.Packet.MovementType);
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        if (ctx.Connection.Player is null)
        {
            _logger.LogCritical("Cannot move player that does not exist. This is a programmatic failure");
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        _logger.LogDebug("Received movement packet with type {MovementType}", ctx.Packet.MovementType);
        if (ctx.Packet.MovementType == CharacterMovementType.MOVE)
        {
            ctx.Connection.Player.Rotation = ctx.Packet.Rotation * 5;
            ctx.Connection.Player.Goto(ctx.Packet.PositionX, ctx.Packet.PositionY, ctx.Connection.Server.ServerTime);
        }
        else
        {
            if (ctx.Packet.MovementType is CharacterMovementType.ATTACK or CharacterMovementType.COMBO)
            {
                // todo: cancel mining if actually mining
                // todo: clears some affects (such as invisibility when attacking)
            }
            else if (ctx.Packet.MovementType.HasAnyFlags(CharacterMovementType.SKILL_FLAG))
            {
                var rawMotion = (byte)ctx.Packet.MovementType & MASK_SKILL_MOTION;
                var motion = (ESkill)rawMotion;

                if (!ctx.Connection.Player.IsUsableSkillMotion(motion))
                {
                    _logger.LogError("Player is not allowed to use skill motion {SkillMotion}", motion);
                    ctx.Connection.Close();
                    return Task.CompletedTask;
                }

                // todo: cancel mining if actually mining
            }
        }

        if (ctx.Packet.MovementType == CharacterMovementType.WAIT)
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
            Duration = ctx.Packet.MovementType == CharacterMovementType.MOVE
                ? ctx.Connection.Player.MovementDuration
                : 0
        };

        foreach (var entity in ctx.Connection.Player.NearbyEntities)
        {
            if (entity is PlayerEntity player)
            {
                player.Connection.Send(movement);
            }
        }

        return Task.CompletedTask;
    }
}
