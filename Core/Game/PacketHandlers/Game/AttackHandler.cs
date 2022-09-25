using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.Core.Networking;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Game;

public class AttackHandler : IPacketHandler<Attack>
{
    private readonly ILogger<AttackHandler> _logger;

    public AttackHandler(ILogger<AttackHandler> logger)
    {
        _logger = logger;
    }
        
    public async Task ExecuteAsync(PacketContext<Attack> ctx, CancellationToken token = default)
    {
        var attacker = ctx.Connection.Player;
        if (attacker == null)
        {
            _logger.LogWarning("Attack without having a player instance");
            ctx.Connection.Close();
            return;
        }
            
        var entity = attacker.Map.GetEntity(ctx.Packet.Vid);
        if (entity == null)
        {
            return;
        }
            
        _logger.LogDebug("Attack from {Attacker} with type {AttackType} target {TargetId}", attacker.Name, ctx.Packet.AttackType, ctx.Packet.Vid);

        await attacker.Attack(entity, 0);
    }
}