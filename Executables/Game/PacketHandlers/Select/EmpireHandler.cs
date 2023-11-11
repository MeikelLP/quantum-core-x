using System.Data;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.PluginTypes;
using QuantumCore.Core.Cache;
using QuantumCore.Database;
using QuantumCore.Game.Packets;

namespace QuantumCore.Game.PacketHandlers.Select;

public class EmpireHandler : IGamePacketHandler<Empire>
{
    private readonly ILogger<EmpireHandler> _logger;
    private readonly ICacheManager _cacheManager;

    public EmpireHandler(ILogger<EmpireHandler> logger, ICacheManager cacheManager)
    {
        _logger = logger;
        _cacheManager = cacheManager;
    }

    public async Task ExecuteAsync(GamePacketContext<Empire> ctx, CancellationToken token = default)
    {
        if (ctx.Packet.EmpireId is > 0 and < 4)
        {
            // Add player to cache
            await _cacheManager.Set("empire:" + ctx.Connection.AccountId, ctx.Packet.EmpireId);
        }
        else
        {
            _logger.LogWarning("Unexpected empire choice {Empire}", ctx.Packet.EmpireId);
        }
    }
}