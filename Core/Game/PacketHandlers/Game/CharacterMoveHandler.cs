using System.Threading;
using System.Threading.Tasks;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;
using QuantumCore.Game.World.Entities;
using Serilog;

namespace QuantumCore.Game.PacketHandlers.Game;

public class CharacterMoveHandler : IPacketHandler<CharacterMove>
{
    public async Task ExecuteAsync(PacketContext<CharacterMove> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.MovementType > (int) CharacterMove.CharacterMovementType.Max &&
            ctx.Packet.MovementType != (int) CharacterMove.CharacterMovementType.Skill)
        {
            Log.Error($"Received unknown movement type ({ctx.Packet.MovementType})");
            ctx.Connection.Close();
            return;
        }
            
        Log.Debug($"Received movement packet with type {(CharacterMove.CharacterMovementType)ctx.Packet.MovementType}");
        if (ctx.Packet.MovementType == (int) CharacterMove.CharacterMovementType.Move)
        {
            ctx.Connection.Player.Rotation = ctx.Packet.Rotation * 5;
            await ctx.Connection.Player.Goto(ctx.Packet.PositionX, ctx.Packet.PositionY);
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
            
        await ctx.Connection.Player.ForEachNearbyEntity(async entity =>
        {
            if(entity is PlayerEntity player)
            {
                await player.Connection.Send(movement);
            }
        });
    }
}