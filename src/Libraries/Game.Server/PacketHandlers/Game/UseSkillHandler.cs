using Microsoft.Extensions.Logging;
using QuantumCore.Packets;
using QuantumCore.PluginTypes;

namespace QuantumCore.Game.PacketHandlers.Game;

public class UseSkillHandler : IGamePacketHandler<PlayerUseSkill>
{
    private readonly ILogger<UseSkillHandler> _logger;

    public UseSkillHandler(ILogger<UseSkillHandler> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(GamePacketContext<PlayerUseSkill> ctx, CancellationToken token = default)
    {
        _logger.LogWarning("SkillId: {SkillId} on TargetVid: {TargetVid} not implemented", ctx.Packet.SkillId,
            ctx.Packet.TargetVid);
        return Task.CompletedTask;
    }
}
