using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Core.Timekeeping;
using QuantumCore.API.PluginTypes;
using QuantumCore.Game.Packets;

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
        if (ctx.Connection.Player is { Timeline: var timeline })
        {
            timeline[PlayerTimestampKind.USED_SKILL] = ctx.Connection.Server.Clock.Now;
        }

        _logger.LogWarning("SkillId: {SkillId} on TargetVid: {TargetVid} not implemented", ctx.Packet.SkillId,
            ctx.Packet.TargetVid);
        return Task.CompletedTask;
    }
}
