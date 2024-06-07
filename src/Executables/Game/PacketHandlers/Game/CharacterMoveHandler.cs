using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;

namespace QuantumCore.Game.PacketHandlers.Game;

public class CharacterMoveHandler : IGamePacketHandler<CharacterMove>
{
    private readonly ILogger<CharacterMoveHandler> _logger;
    
    private const int MaskSkillMotion = 127;

    public CharacterMoveHandler(ILogger<CharacterMoveHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<CharacterMove> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.MovementType >= (int) CharacterMove.CharacterMovementType.Max && (ctx.Packet.MovementType & (byte) CharacterMove.CharacterMovementType.Skill) == 0)
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

        _logger.LogDebug("Received movement packet with type {MovementType}", (CharacterMove.CharacterMovementType)ctx.Packet.MovementType);
        if (ctx.Packet.MovementType == (int) CharacterMove.CharacterMovementType.Move)
        {
            ctx.Connection.Player.Rotation = ctx.Packet.Rotation * 5;
            ctx.Connection.Player.Goto(ctx.Packet.PositionX, ctx.Packet.PositionY);
        }
        else
        {
            if (ctx.Packet.MovementType is (int) CharacterMove.CharacterMovementType.Attack or (int) CharacterMove.CharacterMovementType.Combo)
            {
                // todo: cancel mining if actually mining
                // todo: clears some affects (such as invisibility when attacking)
            }
            else if ((ctx.Packet.MovementType & (byte) CharacterMove.CharacterMovementType.Skill) != 0)
            {
                var motion = ctx.Packet.MovementType & MaskSkillMotion;
            
                if (!ctx.Connection.Player.IsUsableSkillMotion(motion))
                {
                    _logger.LogError("Player is not allowed to use skill motion {SkillMotion}", motion);
                    ctx.Connection.Close();
                    return Task.CompletedTask;
                }
                
                // todo: cancel mining if actually mining
            }
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
        }
        return Task.CompletedTask;
    }
}
