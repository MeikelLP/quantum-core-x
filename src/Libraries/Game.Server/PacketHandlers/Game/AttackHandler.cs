using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class AttackHandler : IGamePacketHandler<Attack>
{
    private readonly ILogger<AttackHandler> _logger;

    public AttackHandler(ILogger<AttackHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<Attack> ctx, CancellationToken token = default)
    {
        var attacker = ctx.Connection.Player;
        if (attacker == null)
        {
            _logger.LogWarning("Attack without having a player instance");
            ctx.Connection.Close();
            return Task.CompletedTask;
        }

        var entity = attacker.Map?.GetEntity(ctx.Packet.Vid);
        if (entity == null)
        {
            return Task.CompletedTask;
        }

        _logger.LogDebug("Attack from {Attacker} with type {SkillMotion} target {TargetId}", attacker.Name,
            ctx.Packet.SkillMotion, ctx.Packet.Vid);

        attacker.Attack(entity);
        return Task.CompletedTask;
    }
}
