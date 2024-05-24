using Microsoft.Extensions.Logging;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

[PacketHandler(typeof(Attack))]
public class AttackHandler
{
    private readonly ILogger<AttackHandler> _logger;

    public AttackHandler(ILogger<AttackHandler> logger)
    {
        _logger = logger;
    }

    public void Execute(GamePacketContext ctx, Attack packet)
    {
        var attacker = ctx.Connection.Player;
        if (attacker == null)
        {
            _logger.LogWarning("Attack without having a player instance");
            ctx.Connection.Close();
            return;
        }

        var entity = attacker.Map?.GetEntity(packet.Vid);
        if (entity == null)
        {
            return;
        }

        _logger.LogDebug("Attack from {Attacker} with type {AttackType} target {TargetId}", attacker.Name,
            packet.AttackType, packet.Vid);

        attacker.Attack(entity, 0);
    }
}